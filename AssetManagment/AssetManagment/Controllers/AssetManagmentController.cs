using AssetManagment.DTO;
using Microsoft.AspNetCore.Mvc;
using AssetManagment.DBLayer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/")]
public class AssetManagementController : ControllerBase
{
 

    private readonly ILogger<AssetManagementController> _logger;

    private readonly JwtSettings _jwtSettings;

    // Inject IConfiguration to access the JwtSettings from appsettings.json
    public AssetManagementController(ILogger<AssetManagementController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
    }
    // 1. Create Asset and Define Approvers
    [HttpGet("Setup")]
    public IActionResult Setup()
    {

        //  UserSecretsConfigurationExtensions.
        User u1 = new User();
        u1.UserName = "x1";
        u1.UserID = 1;

        User u2 = new User();
        u2.UserName = "x2";
        u2.UserID = 2;
        User u3= new User();
        u3.UserName = "x3";
        u3.UserID = 3;
        Repository.Users.Add(u1);
        Repository.Users.Add(u2);
        Repository.Users.Add(u3);

        //Asset
        Asset a1 = new Asset();
        a1.AssetID = 1;
        a1.AssetDescription = "Laptop";
        a1.AssetExpiry = DateTime.UtcNow.AddDays(365);
        a1.MinApproval = 2;
        a1.Approver = new List<AssetApprovers>();
        Repository.AssetApprovers.Add(new AssetApprovers() { ApproverID = 2, AssetID = 1 });

        Repository.AssetApprovers.Add(new AssetApprovers() { ApproverID = 3, AssetID = 1 });
      
        Repository.Assets.Add(a1);


        return Ok();

    }
    [Authorize]
    [HttpPost("create-asset")]
    public IActionResult CreateAsset([FromBody] CreateAssetRequest request)
    {
        try
        {
            var asset = request.Asset;
            var approverIds = request.ApproverIds;

            if (Repository.Assets.Any(a => a.AssetID == asset.AssetID))
            {
                _logger.LogWarning("Asset with ID {AssetID} already exists.", asset.AssetID);
                return Conflict("Asset already exists.");
            }

            Repository.Assets.Add(asset);
            int approverCount = 0;
            foreach (var approverId in approverIds)
            {
                if (!Repository.Users.Any(u => u.UserID == approverId))
                {
                    _logger.LogWarning("Approver with ID {ApproverID} does not exist.", approverId);
                    return NotFound($"Approver with ID {approverId} does not exist.");
                }

                Repository.AssetApprovers.Add(new AssetApprovers
                {
                    AssetID = asset.AssetID,
                    ApproverID = approverId
                });
                approverCount++;
            }

            if(approverCount<asset.MinApproval)
            {
                Repository.Assets.Remove(asset);
                _logger.LogWarning("Minimum approval mismatch");
                return NotFound($"Asset doesnot meet minimum approver criteria.");
            }
            _logger.LogInformation("Asset with ID {AssetID} created successfully.", asset.AssetID);
            return CreatedAtAction(nameof(GetAsset), new { id = asset.AssetID }, asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the asset.");
            return StatusCode(500, "Internal server error.");
        }
    }

    // 2. Request Access to an Asset
    [Authorize]
    [HttpPost("request-access")]
    public IActionResult RequestAccess([FromBody] AssetAllocationRequest allocation)
    {
        try
        {
            if (!Repository.Assets.Any(a => a.AssetID == allocation.AssetID))
            {
                _logger.LogWarning("Asset with ID {AssetID} not found.", allocation.AssetID);
                return NotFound("Asset not found.");
            }
            if (!Repository.Users.Any(u => u.UserID == allocation.UserID))
            {
                _logger.LogWarning("User with ID {UserID} not found.", allocation.UserID);
                return NotFound("User not found.");
            }

            allocation.RequestID = Repository.AssetAllocationsRequest.Count + 1;
            allocation.RequestDate = DateTime.Now;
            Repository.AssetAllocationsRequest.Add(allocation);

            _logger.LogInformation("Access request created successfully for AssetID {AssetID} by UserID {UserID}.", allocation.AssetID, allocation.UserID);
            return Ok(allocation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while requesting access to the asset.");
            return StatusCode(500, "Internal server error.");
        }
    }

    // 3. Get List of Requests to Be Approved
    [Authorize]
    [HttpGet("pending-requests/{approverId}")]
    public IActionResult GetPendingRequests(int approverId)
    {
        try
        {
            var requests = Repository.AssetAllocationsRequest
                .Where(a =>Repository.AssetApprovers.Any(aa =>                 
                aa.ApproverID == approverId && aa.AssetID == a.AssetID))
                .ToList();

            _logger.LogInformation("Retrieved {Count} pending requests for ApproverID {ApproverID}.", requests.Count, approverId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving pending requests.");
            return StatusCode(500, "Internal server error.");
        }
    }


    // 4. Approve a Request
    [Authorize]
    [HttpPost("approve-request")]
    public IActionResult ApproveRequest([FromBody] AssetAllocationLog log)
    {
        try
        {
            var allocation = Repository.AssetAllocationsRequest.FirstOrDefault(a => a.RequestID == log.RequestID);
            if (allocation == null)
            {
                _logger.LogWarning("Request with ID {RequestID} not found.", log.RequestID);
                return NotFound("Request not found.");
            }

            if (!Repository.AssetApprovers.Any(aa => aa.ApproverID == log.ApproverID && aa.AssetID == allocation.AssetID))
            {
                _logger.LogWarning("Approver with ID {ApproverID} not authorized for AssetID {AssetID}.", log.ApproverID, allocation.AssetID);
                return BadRequest("Approver not authorized for this asset.");
            }

           
            log.ApprovalDate = DateTime.Now;
            Repository.AllocationLogs.Add(log);
            Repository.Approvals.Add(new Approvals { ApprovedOn = DateTime.UtcNow, ApproverID = log.ApproverID, RequestID = log.RequestID });
            Asset asset1= Repository.Assets.Where(a=>a.AssetID == allocation.AssetID).FirstOrDefault();
            
            if(Repository.Approvals.Where(a=>a.RequestID== log.RequestID).Count()>= asset1.MinApproval)
            {
                Repository.Allocations.Add(new AllocatedAsset() { AssetID = allocation.AssetID,UserID=allocation.UserID });
            }

            _logger.LogInformation("RequestID {RequestID} approved by ApproverID {ApproverID}.", log.RequestID, log.ApproverID);
            return Ok("Request approved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while approving the request.");
            return StatusCode(500, "Internal server error.");
        }
    }

    // 5. Get List of Users Having Access to Asset A
    [Authorize]
    [HttpGet("users-with-access/{assetId}")]
    public IActionResult GetUsersWithAccess(int assetId)
    {
        try
        {
            var users = Repository.AssetAllocationsRequest
                .Where(a => a.AssetID == assetId)
                .Select(a => Repository.Users.FirstOrDefault(u => u.UserID == a.UserID))
                .Where(u => u != null)
                .ToList();

            _logger.LogInformation("Retrieved {Count} users with access to AssetID {AssetID}.", users.Count, assetId);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving users with access to the asset.");
            return StatusCode(500, "Internal server error.");
        }
    }

    // 6. Get List of All Assets a User U1 Has Access To
    [Authorize]
    [HttpGet("assets-for-user/{userId}")]
    public IActionResult GetAssetsForUser(int userId)
    {
        try
        {
            var assets = Repository.AssetAllocationsRequest
                .Where(a => a.UserID == userId)
                .Select(a => Repository.Assets.FirstOrDefault(asset => asset.AssetID == a.AssetID))
                .Where(asset => asset != null)
                .ToList();

            _logger.LogInformation("Retrieved {Count} assets for UserID {UserID}.", assets.Count, userId);
            return Ok(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving assets for the user.");
            return StatusCode(500, "Internal server error.");
        }
    }
    [Authorize]
    [HttpGet("asset/{id}")]
    public IActionResult GetAsset(int id)
    {
        try
        {
            var asset = Repository.Assets.FirstOrDefault(a => a.AssetID == id);
            if (asset == null)
            {
                _logger.LogWarning("Asset with ID {AssetID} not found.", id);
                return NotFound("Asset not found.");
            }

            _logger.LogInformation("Retrieved asset with ID {AssetID}.", id);
            return Ok(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the asset.");
            return StatusCode(500, "Internal server error.");
        }
    }

    // 7. Add User
    [HttpPost("add-user")]
    public IActionResult AddUser([FromBody] User user)
    {
        try
        {
            if (Repository.Users.Any(u => u.UserID == user.UserID))
            {
                _logger.LogWarning("User with ID {UserID} already exists.", user.UserID);
                return Conflict("User already exists.");
            }

            Repository.Users.Add(user);
            _logger.LogInformation("User with ID {UserID} added successfully.", user.UserID);
            return CreatedAtAction(nameof(GetUser), new { id = user.UserID }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding the user.");
            return StatusCode(500, "Internal server error.");
        }
    }
    [Authorize]
    [HttpGet("user/{id}")]
    public IActionResult GetUser(int id)
    {
        try
        {
            var user = Repository.Users.FirstOrDefault(u => u.UserID == id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserID} not found.", id);
                return NotFound("User not found.");
            }

            _logger.LogInformation("Retrieved user with ID {UserID}.", id);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the user.");
            return StatusCode(500, "Internal server error.");
        }
    }

    // 8. Authenticate User and Generate JWT Token
    [HttpPost("authenticate")]
    public IActionResult Authenticate([FromBody] User user)
    {
        try
        {
            var existingUser = Repository.Users.FirstOrDefault(u => u.UserID == user.UserID && u.UserName == user.UserName);
            if (existingUser == null)
            {
                _logger.LogWarning("Authentication failed for UserID {UserID}.", user.UserID);
                return Unauthorized("Invalid credentials.");
            }

            // Generate JWT token
            var claims = new[]
            {
                    new Claim(ClaimTypes.NameIdentifier, existingUser.UserID.ToString()),
                    new Claim(ClaimTypes.Name, existingUser.UserName),
                    // Add more claims if needed
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "YourIssuer",
                audience: "YourAudience",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("User with ID {UserID} authenticated successfully.", user.UserID);
            return Ok(new { token = tokenString });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during authentication.");
            return StatusCode(500, "Internal server error.");
        }
    }

}



# AssetManagment
Below is the generated documentation for the provided `AssetManagementController` code:

---

## **Asset Management API**

### **Overview**
The `AssetManagementController` is an API controller for managing assets, user authentication, and asset access requests. It provides endpoints to handle asset creation, user management, authentication, access requests, approvals, and related operations.

---

### **Endpoints**

#### **1. Setup**
- **URL**: `GET /api/Setup`
- **Description**: Initializes assets and users in the repository for testing purposes.
- **Response**: 
  - `200 OK`: Setup completed successfully.

---

#### **2. Create Asset**
- **URL**: `POST /api/create-asset`
- **Authorization**: Required
- **Request Body**:
  ```json
  {
      "Asset": {
          "AssetID": 1,
          "AssetDescription": "Laptop",
          "AssetExpiry": "2025-12-31T00:00:00Z",
          "MinApproval": 2
      },
      "ApproverIds": [2, 3]
  }
  ```
- **Response**:
  - `201 Created`: Asset created successfully.
  - `409 Conflict`: Asset already exists.
  - `404 Not Found`: Approver does not exist or minimum approval criteria not met.
  - `500 Internal Server Error`: Unexpected error.

---

#### **3. Request Access to an Asset**
- **URL**: `POST /api/request-access`
- **Authorization**: Required
- **Request Body**:
  ```json
  {
      "AssetID": 1,
      "UserID": 1
  }
  ```
- **Response**:
  - `200 OK`: Access request created successfully.
  - `404 Not Found`: Asset or user not found.
  - `500 Internal Server Error`: Unexpected error.

---

#### **4. Get Pending Requests**
- **URL**: `GET /api/pending-requests/{approverId}`
- **Authorization**: Required
- **Path Parameter**: `approverId` - ID of the approver.
- **Response**:
  - `200 OK`: List of pending requests.
  - `500 Internal Server Error`: Unexpected error.

---

#### **5. Approve Request**
- **URL**: `POST /api/approve-request`
- **Authorization**: Required
- **Request Body**:
  ```json
  {
      "RequestID": 1,
      "ApproverID": 2
  }
  ```
- **Response**:
  - `200 OK`: Request approved successfully.
  - `404 Not Found`: Request not found.
  - `400 Bad Request`: Approver not authorized for the asset.
  - `500 Internal Server Error`: Unexpected error.

---

#### **6. Get Users with Access to an Asset**
- **URL**: `GET /api/users-with-access/{assetId}`
- **Authorization**: Required
- **Path Parameter**: `assetId` - ID of the asset.
- **Response**:
  - `200 OK`: List of users with access.
  - `500 Internal Server Error`: Unexpected error.

---

#### **7. Get Assets for a User**
- **URL**: `GET /api/assets-for-user/{userId}`
- **Authorization**: Required
- **Path Parameter**: `userId` - ID of the user.
- **Response**:
  - `200 OK`: List of assets the user has access to.
  - `500 Internal Server Error`: Unexpected error.

---

#### **8. Add User**
- **URL**: `POST /api/add-user`
- **Request Body**:
  ```json
  {
      "UserID": 4,
      "UserName": "newUser"
  }
  ```
- **Response**:
  - `201 Created`: User added successfully.
  - `409 Conflict`: User already exists.
  - `500 Internal Server Error`: Unexpected error.

---

#### **9. Get User**
- **URL**: `GET /api/user/{id}`
- **Authorization**: Required
- **Path Parameter**: `id` - ID of the user.
- **Response**:
  - `200 OK`: User details.
  - `404 Not Found`: User not found.
  - `500 Internal Server Error`: Unexpected error.

---

#### **10. Authenticate User**
- **URL**: `POST /api/authenticate`
- **Request Body**:
  ```json
  {
      "UserID": 1,
      "UserName": "x1"
  }
  ```
- **Response**:
  - `200 OK`: JWT token.
  - `401 Unauthorized`: Invalid credentials.
  - `500 Internal Server Error`: Unexpected error.

---

#### **11. Get Asset**
- **URL**: `GET /api/asset/{id}`
- **Authorization**: Required
- **Path Parameter**: `id` - ID of the asset.
- **Response**:
  - `200 OK`: Asset details.
  - `404 Not Found`: Asset not found.
  - `500 Internal Server Error`: Unexpected error.

---

### **Error Handling**
- Proper logging is implemented for all operations to aid in debugging and monitoring.
- Meaningful HTTP status codes are returned for different scenarios.

### **Security**
- Endpoints require JWT-based authorization for protected routes.
- Authentication tokens are generated and validated using JWT settings.

---

This documentation provides a clear guide to the API's functionality, making it easier for developers to integrate and use the Asset Management system.

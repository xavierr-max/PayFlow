# PayFlow API Documentation

## Base URL
```
http://localhost:5000
```

## Authentication
Currently, no authentication is required. In a production environment, JWT tokens should be implemented.

## Response Format
All responses are in JSON format with proper HTTP status codes.

---

## Endpoints

### Health Check

#### GET /api/health
Check if the API is running.

**Response:** 200 OK
```json
{
  "status": "ok",
  "timestamp": "2026-05-28T00:38:37.5024268Z"
}
```

---

## Categories

### GET /api/categories
Retrieve all product categories.

**Response:** 200 OK
```json
[
  {
    "id": "vestuario",
    "name": "Vestuário",
    "color": "#F37020"
  },
  {
    "id": "beleza",
    "name": "Beleza",
    "color": "#FF2D55"
  },
  {
    "id": "calcados",
    "name": "Calçados",
    "color": "#0A84FF"
  },
  {
    "id": "acessorios",
    "name": "Acessórios",
    "color": "#FF9F0A"
  },
  {
    "id": "eletronicos",
    "name": "Eletrônicos",
    "color": "#00C853"
  }
]
```

---

## Sellers

### POST /api/sellers
Create a new seller.

**Request Body:**
```json
{
  "name": "João Silva",
  "storeName": "Loja do João",
  "pixKey": "joao@pix.com.br"
}
```

**Response:** 201 Created
```json
{
  "id": "019e6c04-eeaa-7389-b150-6317a7e30ea3",
  "name": "João Silva",
  "storeName": "Loja do João",
  "pixKey": "joao@pix.com.br"
}
```

### GET /api/sellers/{id}
Retrieve a seller by ID.

**Parameters:**
- `id` (UUID): Seller ID

**Response:** 200 OK
```json
{
  "id": "019e6c04-eeaa-7389-b150-6317a7e30ea3",
  "name": "João Silva",
  "storeName": "Loja do João",
  "pixKey": "joao@pix.com.br"
}
```

### PUT /api/sellers/{id}
Update a seller.

**Parameters:**
- `id` (UUID): Seller ID

**Request Body:**
```json
{
  "name": "João Silva Updated",
  "storeName": "Loja do João - Nova",
  "pixKey": "joao.novo@pix.com.br"
}
```

**Response:** 200 OK
```json
{
  "id": "019e6c04-eeaa-7389-b150-6317a7e30ea3",
  "name": "João Silva Updated",
  "storeName": "Loja do João - Nova",
  "pixKey": "joao.novo@pix.com.br"
}
```

### DELETE /api/sellers/{id}
Delete a seller.

**Parameters:**
- `id` (UUID): Seller ID

**Response:** 204 No Content

---

## Products

### POST /api/sellers/{sellerId}/products
Create a new product for a seller.

**Parameters:**
- `sellerId` (UUID): Seller ID

**Request Body:**
```json
{
  "name": "Camiseta Premium",
  "description": "100% algodão",
  "price": 59.90,
  "quantity": 50,
  "categoryId": "vestuario",
  "imageUrl": "https://via.placeholder.com/200"
}
```

**Response:** 201 Created
```json
{
  "id": "019e6c05-04c6-78d5-a4eb-e77306dcc195",
  "name": "Camiseta Premium",
  "description": "100% algodão",
  "price": 59.90,
  "quantity": 50,
  "sellerId": "019e6c04-eeaa-7389-b150-6317a7e30ea3",
  "categoryId": "vestuario",
  "imageUrl": "https://via.placeholder.com/200"
}
```

### GET /api/sellers/{sellerId}/products
List all products for a seller.

**Parameters:**
- `sellerId` (UUID): Seller ID
- `category` (string, optional): Filter by category ID

**Response:** 200 OK
```json
[
  {
    "id": "019e6c05-04c6-78d5-a4eb-e77306dcc195",
    "name": "Camiseta Premium",
    "description": "100% algodão",
    "price": 59.90,
    "quantity": 50,
    "sellerId": "019e6c04-eeaa-7389-b150-6317a7e30ea3",
    "categoryId": "vestuario",
    "imageUrl": "https://via.placeholder.com/200"
  }
]
```

### GET /api/products/{id}
Retrieve a product by ID.

**Parameters:**
- `id` (UUID): Product ID

**Response:** 200 OK
```json
{
  "id": "019e6c05-04c6-78d5-a4eb-e77306dcc195",
  "name": "Camiseta Premium",
  "description": "100% algodão",
  "price": 59.90,
  "quantity": 50,
  "sellerId": "019e6c04-eeaa-7389-b150-6317a7e30ea3",
  "categoryId": "vestuario",
  "imageUrl": "https://via.placeholder.com/200"
}
```

### PUT /api/products/{id}
Update a product.

**Parameters:**
- `id` (UUID): Product ID

**Request Body:**
```json
{
  "name": "Camiseta Premium Updated",
  "description": "100% algodão - Novo modelo",
  "price": 69.90,
  "quantity": 75,
  "categoryId": "vestuario",
  "imageUrl": "https://via.placeholder.com/200"
}
```

**Response:** 200 OK

### DELETE /api/products/{id}
Delete a product.

**Parameters:**
- `id` (UUID): Product ID

**Response:** 204 No Content

---

## Customers

### POST /api/sellers/{sellerId}/customers
Create a new customer for a seller.

**Parameters:**
- `sellerId` (UUID): Seller ID

**Request Body:**
```json
{
  "name": "Carlos Santos",
  "description": "Cliente VIP",
  "phone": "(11) 98765-4321"
}
```

**Response:** 201 Created
```json
{
  "id": "019e6c05-04db-7350-991e-8cc5b3bacf35",
  "name": "Carlos Santos",
  "description": "Cliente VIP",
  "phone": "(11) 98765-4321",
  "sellerId": "019e6c04-eeaa-7389-b150-6317a7e30ea3"
}
```

### GET /api/sellers/{sellerId}/customers
List all customers for a seller.

**Parameters:**
- `sellerId` (UUID): Seller ID

**Response:** 200 OK
```json
[
  {
    "id": "019e6c05-04db-7350-991e-8cc5b3bacf35",
    "name": "Carlos Santos",
    "description": "Cliente VIP",
    "phone": "(11) 98765-4321",
    "sellerId": "019e6c04-eeaa-7389-b150-6317a7e30ea3"
  }
]
```

### GET /api/customers/{id}
Retrieve a customer by ID.

**Parameters:**
- `id` (UUID): Customer ID

**Response:** 200 OK

### PUT /api/customers/{id}
Update a customer.

**Parameters:**
- `id` (UUID): Customer ID

**Request Body:**
```json
{
  "name": "Carlos Santos Updated",
  "description": "Cliente VIP - Platinum",
  "phone": "(11) 99999-9999"
}
```

**Response:** 200 OK

### DELETE /api/customers/{id}
Delete a customer.

**Parameters:**
- `id` (UUID): Customer ID

**Response:** 204 No Content

---

## Payments

### POST /api/sellers/{sellerId}/payments
Create a new payment for a customer.

**Parameters:**
- `sellerId` (UUID): Seller ID

**Request Body:**
```json
{
  "customerId": "019e6c05-04db-7350-991e-8cc5b3bacf35",
  "amount": 299.90,
  "installmentNumber": 1,
  "totalInstallments": 3,
  "dueDate": "2026-06-27T23:59:59"
}
```

**Response:** 201 Created
```json
{
  "id": "019e6c05-1bad-73f6-82f5-f27efa07ee1c",
  "status": 1,
  "statusLabel": "Pendente",
  "amount": 299.90,
  "installmentNumber": 1,
  "totalInstallments": 3,
  "dueDate": "2026-06-27T23:59:59",
  "paidAt": null,
  "txId": "019e6c051bad7cf68e7a6228b019e124",
  "customerId": "019e6c05-04db-7350-991e-8cc5b3bacf35",
  "customerName": "Carlos Santos"
}
```

### GET /api/sellers/{sellerId}/payments
List all payments for a seller.

**Parameters:**
- `sellerId` (UUID): Seller ID
- `status` (string, optional): Filter by status (Pending, Paid, Overdue)

**Response:** 200 OK
```json
[
  {
    "id": "019e6c05-1bad-73f6-82f5-f27efa07ee1c",
    "status": 1,
    "statusLabel": "Pendente",
    "amount": 299.90,
    "installmentNumber": 1,
    "totalInstallments": 3,
    "dueDate": "2026-06-27T23:59:59",
    "paidAt": null,
    "txId": "019e6c051bad7cf68e7a6228b019e124",
    "customerId": "019e6c05-04db-7350-991e-8cc5b3bacf35",
    "customerName": "Carlos Santos"
  }
]
```

### GET /api/payments/{id}
Retrieve a payment by ID.

**Parameters:**
- `id` (UUID): Payment ID

**Response:** 200 OK

### PUT /api/payments/{id}/mark-paid
Mark a payment as paid.

**Parameters:**
- `id` (UUID): Payment ID

**Request Body:**
```json
{
  "valueReceived": 299.90
}
```

**Response:** 200 OK
```json
{
  "id": "019e6c05-1bad-73f6-82f5-f27efa07ee1c",
  "status": 2,
  "statusLabel": "Pago",
  "amount": 299.90,
  "installmentNumber": 1,
  "totalInstallments": 3,
  "dueDate": "2026-06-27T23:59:59",
  "paidAt": "2026-05-28T00:38:54.6706775Z",
  "txId": "019e6c051bad7cf68e7a6228b019e124",
  "customerId": "019e6c05-04db-7350-991e-8cc5b3bacf35",
  "customerName": "Carlos Santos"
}
```

### DELETE /api/payments/{id}
Delete a payment.

**Parameters:**
- `id` (UUID): Payment ID

**Response:** 204 No Content

---

## Error Handling

### Error Response Format
```json
{
  "message": "Error description",
  "error": "ExceptionType"
}
```

### HTTP Status Codes
- **200 OK**: Successful GET/PUT request
- **201 Created**: Successful POST request
- **204 No Content**: Successful DELETE request
- **400 Bad Request**: Validation error
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server error

### Common Error Scenarios

#### Not Found
```
Status: 404
{
  "message": "Seller with ID 00000000-0000-0000-0000-000000000000 not found",
  "error": "NotFoundException"
}
```

#### Validation Error
```
Status: 400
{
  "message": "Invalid payment amount",
  "error": "ValidationException"
}
```

---

## Running the API

### Development
```bash
cd PayFlow
dotnet run --project PayFlow.API/PayFlow.API.csproj
```

The API will be available at `http://localhost:5000`

### Docker (Future)
```bash
docker build -t payflow-api .
docker run -p 5000:5000 payflow-api
```

---

## Notes

- **IDs**: All resource IDs are UUIDs (Globally Unique Identifiers)
- **Timestamps**: All timestamps are in UTC ISO 8601 format
- **Money**: All monetary values are in Brazilian Real (BRL)
- **Phone Format**: Phone numbers follow the pattern (XX) XXXXX-XXXX
- **Data Persistence**: Currently using in-memory storage. For production, integrate with a database like SQL Server or PostgreSQL

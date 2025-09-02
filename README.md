# E-CommerceSystem (ASP.NET Core + EF Core)

A simple e-commerce **Web API** for managing users, products, orders, and reviews.
It’s designed with clear layers (Controllers → Services → Repositories → EF Core) and ready for local development.

---

## 1) What is this? (Non-technical overview)

This system provides the **backend** (no mobile/web UI included) for an online shop:

* **Users** can register, log in, browse products, place orders, and leave reviews.
* The system keeps track of **products**, **orders**, and **reviews** in a secure database.
* Actions like adding reviews or placing orders require the user to log in and provide a secure token (JWT).

---

## 2) Data model (What data do we store?)

* **Users**: name, email, password, phone, created date
* **Products**: name, description, price, stock, overall rating
* **Orders**: who ordered, when, how much
* **OrderProducts**: which products were in an order and how many
* **Reviews**: user feedback (rating + comment) for a product

> Relationships:
> A **User** has many **Orders** and **Reviews**.
> An **Order** has many **Products** (via **OrderProducts**).
> A **Product** has many **Reviews** and many **Orders** (via **OrderProducts**).

---

## 3) API overview

Explore endpoints via Swagger when you run the project.

### Users

* `POST /api/User/Register` – create account
* `POST /api/User/Login` – returns JWT token

### Products

* `GET /api/Product/GetAllProducts` – list (supports paging/filtering)
* `GET /api/Product/GetProductByID/{id}` – details
* `POST /api/Product/AddProduct` – add product
* `PUT /api/Product/UpdateProduct` – update product

### Orders

* `POST /api/Order/PlaceOrder` – place an order
* `GET /api/Order/GetAllOrders` – list all orders

### Reviews

* `POST /api/Review/AddReview` – add review
* `GET /api/Review/GetAllReviews` – all reviews

---

## 4) Project structure

```
E-CommerceSystem/
  Controllers/        # HTTP endpoints
  Services/           # Business logic
  Repositories/       # Data access contracts + EF implementations
  Models/             # Entities + DTOs
  Migrations/         # EF Core migrations
  ApplicationDbContext.cs
  Program.cs
  appsettings*.json
```

* **Controllers** handle incoming requests.
* **Services** implement business logic (e.g., order placement).
* **Repositories** interact with the database via **ApplicationDbContext**.

---

## 5) Security & data quality

* **JWT Authentication** is used for login and protected actions.
* **Data Annotations** validate inputs (email format, required fields, rating range).
* **Unique Email** enforced in the database.

---
  
## 6) DbSets in ApplicationDbContext
public DbSet<User> Users { get; set; }
public DbSet<Product> Products { get; set; }
public DbSet<Order> Orders { get; set; }
public DbSet<OrderProducts> OrderProducts { get; set; }
public DbSet<Review> Reviews { get; set; }

---


## 7) Data Annotations used in models

[Key] → marks primary keys

[Required] → required fields (e.g., UName, Email, Password, Phone)

[Range] → numeric validation (Price, Stock, Rating)

[RegularExpression] → email format & password strength

[ForeignKey] → links Order, Review, and OrderProducts to related entities

[JsonIgnore] → prevents circular references in API responses


---


## 8) Fluent API used in DbContext

the using of it :

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique(); // ensures each email is unique
}



---

# Nectarine API

Welcome to Nectarine, my personal project that I've been sporadically working on for a number of years. It's an e-commerce API, featuring authentication, product generation, Email and SMS messaging using SendGrid, Stripe integration, JWT, among more.

## Features

### 1. Test Coverage

The primary motivation for starting this project is that I wanted to create a project that was TDD from the start. This project has 79% coverage, 100% controller coverage and 93% Service coverage, the remaining 7% being what I believe to be untestable, 3rd party code, (Can't mock out a SendGrid client).

![Tests](https://github.com/stefannovak/Nectarine/assets/18539857/a5f85485-ad2c-45a9-bfb0-1bd99bd2146d)

### 2. Products

Nectarine features a dynamic product generator that creates random types of clothing during the first startup. This is to mimic real items. Product images aren't generated yet however.

### 3. Data

Nectarine uses Entity Framework to interact with a database, which was at a first an Azure SQL database but is now a Fly PostGRES database to save costs. There's a small relational model, including entities for Orders, Products, Ratings, and Users, with Users following ApplicationUsers for Identity.

### 4. Communication Services

Nectarine supports both email and SMS services using SendGrid. You'll get welcome emails upon signing up and confirming orders, as well as SMS confirmation for 2FA.

### 5. Authentication and Authorization

Leverage .NET Identity accounts for secure user authentication. Nectarine goes further by offering Microsoft, Facebook, and Google sign-in options, enhancing user experience and accessibility.

### 6. Payment Integration

The Stripe API is used. I wanted to store most of the sensitive data for my users to Stripe because I know they handle security very well. Stripe is more useful on the front end though. There's a Payment Intents controller in this API.

### 7. JWT Authentication

JWT auth is implemented. Sign in with a social sign in, or an email and password and you'll recieve a JWT needed for most other endpoints.

## Getting Started

To get started with Nectarine, just go to the ![swagger page](https://nectarine-dev.fly.dev/swagger/index.html)! 

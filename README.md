# BankWebApp Readme

This is a web application for managing banking operations. It is built using ASP.NET Core and utilizes a provided database, which can be accessed through the provided link. The Entity Framework Core is used to create entities and a DbContext that corresponds to the database.

Best practices and principles are followed throughout the application, particularly with regards to ViewModels. No database entities should be directly used in the views.

Key Features:

1. Home Page: Displays statistics such as the number of customers, number of accounts, and the total balance across all accounts. This page is publicly accessible and does not require authentication.

2. Customer Profile: Users can retrieve a customer profile by entering the customer number. The profile displays all information about the customer, including their accounts and the total balance by summing up all account balances.

3. Customer Search: Users can search for customers based on name and city. The search results display a paginated list showing customer numbers, social security numbers, names, addresses, and cities. Clicking on a customer will redirect to their customer profile.

4. Account Profile: Users can view transactions for an individual account by clicking on the account number in the customer profile. The account page displays the account number, balance, and a list of transactions in descending order of date. If there are more than 20 transactions, JavaScript/AJAX is used to load an additional 20 transactions when the user clicks a button at the bottom of the list. Clicking the button again will load another 20 transactions, and so on.

5. Transaction Management: The system supports deposits, withdrawals, and transfers between accounts. All changes to the account balance must be made through transactions to avoid rounding errors. The application ensures that users are notified if they try to purchase or transfer more money than what is available in the account.

6. ASP.NET Core Identity: The application incorporates ASP.NET Core Identity for user authentication and authorization. Two users are pre-seeded into the system with their roles:
   - richard.chalk@systementor.se (Admin)
   - richard.erdos.chalk@gmail.se (Cashier)

   Both users have the same password: Hejsan123#. The roles "Admin" and "Cashier" are automatically created in the database upon startup if they do not already exist. The "Cashier" role has the ability to manage customers and their accounts.

Please ensure that the required dependencies, such as AutoMapper, class libraries, ReadMe.md, and input validation, are implemented as per best practices. The application is designed to cater to real-world scenarios with non-technical users.

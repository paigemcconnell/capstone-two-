﻿using System;
using TenmoClient.Data;
using TenmoClient.APIClients;
using System.Collections.Generic;

namespace TenmoClient
{
    public class UserInterface
    {
        private readonly ConsoleService consoleService = new ConsoleService();
        private readonly AuthService authService = new AuthService();
        private TransferRestClient transferClient = new TransferRestClient();
        private bool quitRequested = false;

        public void Start()
        {
            while (!quitRequested)
            {
                while (!authService.IsLoggedIn)
                {
                    ShowLogInMenu();
                }

                // If we got here, then the user is logged in. Go ahead and show the main menu
                ShowMainMenu();
            }
        }

        private void ShowLogInMenu()
        {
            Console.WriteLine("Welcome to TEnmo!");
            Console.WriteLine("1: Login");
            Console.WriteLine("2: Register");
            Console.Write("Please choose an option: ");

            if (!int.TryParse(Console.ReadLine(), out int loginRegister))
            {
                Console.WriteLine("Invalid input. Please enter only a number.");
            }
            else if (loginRegister == 1)
            {
                HandleUserLogin();
            }
            else if (loginRegister == 2)
            {
                HandleUserRegister();
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }

        private void ShowMainMenu()
        {
            int menuSelection;
            do
            {
                Console.WriteLine();
                Console.WriteLine("Welcome to TEnmo! Please make a selection: ");
                Console.WriteLine("1: View your current balance");
                Console.WriteLine("2: View your past transfers");
                Console.WriteLine("3: View your pending requests");
                Console.WriteLine("4: Send TE bucks");
                Console.WriteLine("5: Request TE bucks");
                Console.WriteLine("6: Log in as different user");
                Console.WriteLine("0: Exit");
                Console.WriteLine("---------");
                Console.Write("Please choose an option: ");

                if (!int.TryParse(Console.ReadLine(), out menuSelection))
                {
                    Console.WriteLine("Invalid input. Please enter only a number.");
                }
                else
                {
                    switch (menuSelection)
                    {
                        case 1: // View Balance
                            DisplayBalance();
                            break;

                        case 2: // View Past Transfers
                            DisplayTransfers();
                            Console.WriteLine("Enter transfer ID to view details: ");
                            string transferIdInput = Console.ReadLine();
                            int transferId = Int32.Parse(transferIdInput);
                            GetTransferDetails(transferId);
                            break;

                        case 3: // View Pending Requests
                            Console.WriteLine("This feature is coming soon!"); // TODO: Implement me
                            break;

                        case 4: // Send TE Bucks
                            DisplayUsers();
                            SendFunds();
                            break;

                        case 5: // Request TE Bucks
                            Console.WriteLine("This feature is coming soon!"); // TODO: Implement me
                            break;

                        case 6: // Log in as someone else
                            authService.ClearAuthenticator();
                            // NOTE: You will need to clear any stored JWTs in other API Clients
                            transferClient.UpdateToken(null);
                            return; // Leaves the menu and should return as someone else

                        case 0: // Quit
                            Console.WriteLine("Goodbye!");
                            quitRequested = true;
                            return;

                        default:
                            Console.WriteLine("That doesn't seem like a valid choice.");
                            break;
                    }
                }
            } while (menuSelection != 0);
        }

        private void HandleUserRegister()
        {
            bool isRegistered = false;

            while (!isRegistered) //will keep looping until user is registered
            {
                LoginUser registerUser = consoleService.PromptForLogin();
                isRegistered = authService.Register(registerUser);
            }

            Console.WriteLine("");
            Console.WriteLine("Registration successful. You can now log in.");
        }

        private void HandleUserLogin()
        {
            while (!authService.IsLoggedIn) //will keep looping until user is logged in
            {
                LoginUser loginUser = consoleService.PromptForLogin();

                // Log the user in
                API_User authenticatedUser = authService.Login(loginUser);

                if (authenticatedUser == null) // if log in failed 
                {
                    Console.WriteLine("Could not log in");
                }
                else // log in successfull and jwt is stored
                {
                    string jwt = authenticatedUser.Token;
                    transferClient.UpdateToken(jwt);   // token is updated to the current user's jwt

                    Console.WriteLine("Successfully logged in");
                }
            }
        }

        private void DisplayBalance()
        {
            AccountBalance account = transferClient.DisplayBalance();   // this calls the RestClient method to display balance
            Console.WriteLine("Your current account balance is: " + account.Balance.ToString("C"));
        }

        private List<API_User> DisplayUsers()
        {
            List<API_User> users = transferClient.DisplayUsers();

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Users");
            Console.WriteLine("ID" + ("Name".PadLeft(5)));
            Console.WriteLine("--------------------------------------------");

            foreach(API_User user in users)
            {
            Console.WriteLine($"{user.UserId} {user.Username.PadRight(20)}");
            }
            return users;
        }

        private void SendFunds()
        {
            Console.WriteLine("Enter ID of user you are sending to:");
            string userIdInput = Console.ReadLine();
            int userId = Int32.Parse(userIdInput);

            Console.WriteLine("Enter the amount to transfer:");
            string amountInput = Console.ReadLine();                       
            decimal amount = Decimal.Parse(amountInput);                         
            API_User user = new API_User();                                // new instance of API_User (the user who is logged in)
            Transfer transfer = new Transfer();                            // instantiate a new transfer
            transfer.UsersFromId = user.UserId;                            // set the User From ID to the logged in user's ID (API_User)
            transfer.UsersToId = userId;                                   // set the User To ID to the input user ID
            transfer.TransferAmount = amount;                              // set the Amount to the input amount
            bool completed = transferClient.SendFunds(transfer);           //calls the API Client method SendFunds return a bool
            
            if (completed)
            {
                Console.WriteLine("Transfer successful!");
            }
            else
            {
                Console.WriteLine();
            }

        }

        private List<Transfer> DisplayTransfers()
        {
            List<Transfer> transfers = transferClient.DisplayTransfers();

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Transfers");
            Console.WriteLine("ID       " + ("From:     ") + ("To:       ") + ("Amount:     "));
            Console.WriteLine("--------------------------------------------");

            foreach (Transfer transfer in transfers)
            {
                Console.WriteLine($"{transfer.TransferId}     {transfer.UserFrom}     {transfer.UserTo}     {transfer.Amount.ToString("C")}");
               
            }
            Console.WriteLine();
            return transfers;

        }

        private Transfer GetTransferDetails(int transferId)
        {

            Transfer transfer = transferClient.GetTransferDetails(transferId);

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Transfer Details");
            Console.WriteLine("--------------------------------------------");
            
            Console.WriteLine("Id: " + transfer.TransferId);
            Console.WriteLine("From: " + transfer.UserFrom);
            Console.WriteLine("To: " + transfer.UserTo);
            Console.WriteLine("Type: Send");
            Console.WriteLine("Status: Approved");
            Console.WriteLine("Amount: " + transfer.Amount.ToString("C"));
            Console.WriteLine(); 
            return transfer; 
        }

    }
}

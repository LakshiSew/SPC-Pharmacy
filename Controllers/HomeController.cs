using Newtonsoft.Json;
using SPC_Pharmacy.Models;
using SPC_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SPC_Pharmacy.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Home()
        {
            return View();
        }
        public ActionResult Index()
        {
            return View();
        }

        /////////////////////////////////////Pharmacy login////////////////////////////////////////////////////
        [HttpPost]
        public async Task<ActionResult> Index(Pharmacy pharmacy)
        {
            if (ModelState.IsValid)
            {
                using (HttpClient client = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(pharmacy), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("https://localhost:44307/api/Pharmacies/Login", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                        // Set success message in TempData
                        TempData["SuccessMessage"] = "Login successful! Welcome to the Pharmacy Dashboard.";

                        // Redirect based on user role

                        return RedirectToAction("Home");

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid username or password.";
                    }
                }
            }
            return View(pharmacy);
        }

        //////////////////////////////////////////////////////////////// pharmacy signup /////////////////////////////////////////////////////////////////////

        public async Task<ActionResult> AddPharmacy()
        {
            return View();
        }

        // POST: AddSupplier
        [HttpPost]
        public async Task<ActionResult> AddPharmacy(Pharmacy pr)
        {
            if (!ModelState.IsValid)
            {
                return View(pr); 
            }

            // Get the next PharmacyID
            int nextPharmacyId;
            using (var httpClient = new HttpClient())
            {
                // Fetch existing pharmacies to determine the next ID
                var response = await httpClient.GetAsync("https://localhost:44307/api/Pharmacies");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadAsStringAsync();
                    var pharmacies = JsonConvert.DeserializeObject<List<Pharmacy>>(apiResponse);
                    nextPharmacyId = pharmacies.Any() ? pharmacies.Max(p => p.PharmacyID) + 1 : 1;
                }
                else
                {
                  
                    ModelState.AddModelError("", "Unable to retrieve pharmacies.");
                    return View(pr);
                }
            }

            // Set the PharmacyID for the new pharmacy
            pr.PharmacyID = nextPharmacyId;

            using (var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(pr), Encoding.UTF8, "application/json");
                using (var response = await httpClient.PostAsync("https://localhost:44307/api/Pharmacies", content))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Pharmacy added successfully!";
                        return RedirectToAction("AddPharmacy"); 
                    }
                    else
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", "Error adding the Pharmacy: " + apiResponse);
                    }
                }
            }

            return View(pr); 
        }




        //////////////////////////////////////////////// get stocks ////////////////////////////////////////////////////


        public async Task<ActionResult> GetStocks(string searchTerm = null)
        {
            List<StockUpdate> Drugs = new List<StockUpdate>();

            using (HttpClient client = new HttpClient())
            {
                // Pass the searchTerm to the API
                string apiUrl = string.IsNullOrEmpty(searchTerm)
                    ? "https://localhost:44307/api/StockUpdates"
                    : $"https://localhost:44307/api/StockUpdates?searchTerm={searchTerm}";

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    Drugs = JsonConvert.DeserializeObject<List<StockUpdate>>(data);
                }
            }

            // Pass the search term back to the view to retain it in the search box
            ViewBag.SearchTerm = searchTerm;

            return View(Drugs);
        }

        ////////////////////////////////////////////////////// add order ///////////////////////////////////////////

        public async Task<ActionResult> AddOrder(int id)
        {
            StockUpdate stockUpdate = null;

            using (HttpClient client = new HttpClient())
            {
                // Fetch the stock update by ID
                var response = await client.GetAsync($"https://localhost:44307/api/StockUpdates/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    stockUpdate = JsonConvert.DeserializeObject<StockUpdate>(data);
                }
            }

            if (stockUpdate == null)
            {
                return HttpNotFound(); 
            }

            // Create a new OrderViewModel to pass to the view
            var orderViewModel = new OrderViewModel
            {
                DrugName = stockUpdate.DrugName
            };

            // Fetch the max OrderID (auto-increment logic)
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync("https://localhost:44307/api/Orders");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var orders = JsonConvert.DeserializeObject<List<Order>>(data);

                    if (orders.Count > 0)
                    {
                        // Get the maximum OrderID and increment it by 1
                        orderViewModel.OrderID = orders.Max(x => x.OrderID) + 1;
                    }
                    else
                    {
                        orderViewModel.OrderID = 1; // If no orders exist, start with 1
                    }
                }
            }

            return View(orderViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrder(OrderViewModel orderViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("AddOrder", orderViewModel); 
            }

            // Check if OrderID is greater than 0
            if (orderViewModel.OrderID <= 0)
            {
                ModelState.AddModelError("", "Invalid Order ID.");
                return View("AddOrder", orderViewModel); // Return to the view with error message
            }

            // Create a new Order object
            var order = new Order
            {
                OrderID = orderViewModel.OrderID,
                PharmacyName = orderViewModel.PharmacyName,
                DrugName = orderViewModel.DrugName,
                Quantity = orderViewModel.Quantity,
                OrderDate = DateTime.Now,
                Status = "Pending" // Default status
            };

            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://localhost:44307/api/Orders", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Order placed successfully!";
                    return RedirectToAction("GetOrders"); // Redirect to the orders list or another appropriate action
                }
                else
                {
                    ModelState.AddModelError("", "Error placing the order. Please try again.");
                }
            }

            return View("AddOrder", orderViewModel); // Return to the view with the current model
        }



        /// ///////////////////////////////Get OrderHostory///////////////////////////////////////////////////////////////

        public async Task<ActionResult> GetOrders()
        {
            List<Order> spcOrder = new List<Order>();

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://localhost:44307/api/Orders");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    spcOrder = JsonConvert.DeserializeObject<List<Order>>(data);
                }
            }

            return View(spcOrder);
        }



        public ActionResult AboutUs()
        {
            return View(); // Just return the view without passing data
        }




        public ActionResult Logout()
        {
            // Clear the session or authentication cookies
            Session.Clear(); // Clears all session data
            Session.Abandon(); // Optional: Ends the session

            // Redirect to the Index page (or any other page as needed)
            return RedirectToAction("Index", "Home");
        }

    }

}

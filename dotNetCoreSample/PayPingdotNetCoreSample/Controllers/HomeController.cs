using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PayPingdotNetCoreSample.Models;
using PayPingdotNetCoreSample.Models.ExternalModels;
using PayPingdotNetCoreSample.Models.RequestModels;
using PayPingdotNetCoreSample.Models.ResponseModels;

namespace PayPingdotNetCoreSample.Controllers
{
    public class HomeController : Controller
    {
        string PayPing_Token = "b96ad81945a59cd8da34736a96e657390e1e5bf5e372732d3de0a364b1a410cf";
        string PayPing_PayURL = "https://api.payping.ir/v2/pay";
        string Return_URL = "https://localhost:5001/Home/ConfirmPay";
        
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Home Page
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }
        
        /// <summary>
        /// Create A Payment
        /// </summary>
        /// <param name="model">Payment Details</param>
        /// <response code="301">User redirected to Gateway For Payment</response>
        [HttpPost]
        public async Task<IActionResult> CreatePay(CreatePayRequestModel model)
        {

            // Set Return_URL And ClientRefId(Optional Value)
            model.ReturnUrl = Return_URL;
            model.ClientRefId = "YOUR OPTIONAL VALUE";
            
            //Create A HttpClient and add Auth Header to it
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization 
                = new AuthenticationHeaderValue("Bearer", PayPing_Token);
            
            //Convert Request Model to Json String
            var data = JsonConvert.SerializeObject(model);
            var content = new StringContent(data, Encoding.UTF8, "application/json");

            //Send Create pay Request
            var response = await httpClient.PostAsync(PayPing_PayURL, content);
            
            //Check if we ran into an Issue
            response.EnsureSuccessStatusCode();
            
            //Get Response data 
            string responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(responseBody);

            //Convert Response data to Model and get PayCode
            var createPayResult = JsonConvert.DeserializeObject<CreatePayResponseModel>(responseBody);
            
            //Redirect User to GateWay with our PayCodeS
            return Redirect($"{PayPing_PayURL}/gotoipg/{createPayResult.Code}");
        }
        
        /// <summary>
        /// Confirm A Payment
        /// *Gets Called After Payment is Done By PayPing
        /// </summary>
        /// <param name="model">Payment RefId</param>
        /// <response code="301">User redirected to Gateway For Payment</response>
        [HttpPost]
        public async Task<IActionResult> ConfirmPay(PPVerifyPayHookModel model)
        {

            //Create A HttpClient and add Auth Header to it
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization 
                = new AuthenticationHeaderValue("Bearer", PayPing_Token);
            
            var verify_model = new VerifyPayRequestModel()
            {
                Amount = 1000,
                RefId = model.RefId
            };
            
            //Convert Request Model to Json String
            var data = JsonConvert.SerializeObject(verify_model);
            var content = new StringContent(data, Encoding.UTF8, "application/json");

            //Send Verify pay Request
            var response = await httpClient.PostAsync(PayPing_PayURL + "/verify", content);
            
            //Check if we ran into an Issue
            response.EnsureSuccessStatusCode();
            
            //Get Response data 
            string responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(responseBody);

            //Convert Response data to Model and get our Payment Details
            var paymentDetails = JsonConvert.DeserializeObject<VerifyPayResponseModel>(responseBody);
            
            //Show Confirm Page
            return View();
        }

   
        /// <summary>
        /// Handles Error Show
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}
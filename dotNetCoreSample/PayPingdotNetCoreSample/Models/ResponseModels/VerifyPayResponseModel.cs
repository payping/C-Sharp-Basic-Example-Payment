namespace PayPingdotNetCoreSample.Models.ResponseModels
{
    public class VerifyPayResponseModel
    {
        public int Amount { get; set; }
        public string CardNumber { get; set; }
        public string CardHashPan { get; set; }
    }
}
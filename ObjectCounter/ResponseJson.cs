namespace AzureObjCounterS{
    public class ResponseJson {
        public string id { get; set; }
        public string project { get; set; }
        public string iteration { get; set; }
        public DateTime created { get; set; }
        public List<Predictions> predictions { get; set; }
    }
}

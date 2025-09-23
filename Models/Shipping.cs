namespace Proyecto_v1.Models
{
    public class Shipping
    {
        public int ShippingId { get; set; }
        public string TrackingNumber { get; set; }
        public string ShippingStatus { get; set; }
        public double ShippingPrice { get; set; }
        public bool IsActive { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}

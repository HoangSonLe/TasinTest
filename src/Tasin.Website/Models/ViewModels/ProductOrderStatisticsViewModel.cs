using System.ComponentModel.DataAnnotations;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// View model for product order statistics grouped by vendor
    /// </summary>
    public class ProductOrderStatisticsViewModel
    {
        /// <summary>
        /// Vendor information
        /// </summary>
        public VendorStatisticsViewModel Vendor { get; set; } = new();

        /// <summary>
        /// List of products ordered from this vendor
        /// </summary>
        public List<ProductStatisticsViewModel> Products { get; set; } = new();

        /// <summary>
        /// Total value for this vendor (sum of all product values)
        /// </summary>
        public decimal TotalValue { get; set; }

        /// <summary>
        /// Total order amount for this vendor (total money spent)
        /// </summary>
        public decimal TotalOrderAmount { get; set; }

        /// <summary>
        /// Total quantity for this vendor
        /// </summary>
        public decimal TotalQuantity { get; set; }

        /// <summary>
        /// Number of completed PAs for this vendor
        /// </summary>
        public int CompletedPACount { get; set; }
    }

    /// <summary>
    /// Vendor information for statistics
    /// </summary>
    public class VendorStatisticsViewModel
    {
        public int ID { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Address { get; set; }
    }

    /// <summary>
    /// Product statistics information
    /// </summary>
    public class ProductStatisticsViewModel
    {
        public int ProductID { get; set; }
        public string ProductCode { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string? UnitName { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal? MinPrice { get; set; } // Giá thấp nhất
        public decimal? MaxPrice { get; set; } // Giá cao nhất
        public decimal? AveragePrice { get; set; } // Giá trung bình
        public decimal? CurrentPrice { get; set; } // Giá hiện tại (giá gần nhất)
        public decimal TotalValue { get; set; } // Tổng giá trị (quantity * price)
        public decimal TotalOrderAmount { get; set; } // Tổng tiền đặt hàng
        public int PACount { get; set; } // Number of PAs containing this product
        public List<PAProductDetailViewModel> PADetails { get; set; } = new();
    }

    /// <summary>
    /// PA detail for a specific product
    /// </summary>
    public class PAProductDetailViewModel
    {
        public int PA_ID { get; set; }
        public string PACode { get; set; } = "";
        public string GroupCode { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal Value { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// View model for customer product order statistics grouped by customer
    /// </summary>
    public class CustomerProductOrderStatisticsViewModel
    {
        /// <summary>
        /// Customer information
        /// </summary>
        public CustomerStatisticsViewModel Customer { get; set; } = new();

        /// <summary>
        /// List of products ordered by this customer
        /// </summary>
        public List<CustomerProductStatisticsViewModel> Products { get; set; } = new();

        /// <summary>
        /// Total value for this customer (sum of all product values)
        /// </summary>
        public decimal TotalValue { get; set; }

        /// <summary>
        /// Total order amount for this customer (total money spent)
        /// </summary>
        public decimal TotalOrderAmount { get; set; }

        /// <summary>
        /// Number of confirmed POs for this customer
        /// </summary>
        public int ConfirmedPOCount { get; set; }
    }

    /// <summary>
    /// Customer statistics information
    /// </summary>
    public class CustomerStatisticsViewModel
    {
        public int ID { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneContact { get; set; }
        public string? Address { get; set; }
        public string? TaxCode { get; set; }
    }

    /// <summary>
    /// Customer product statistics information
    /// </summary>
    public class CustomerProductStatisticsViewModel
    {
        public int ProductID { get; set; }
        public string ProductCode { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string? UnitName { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal? MinPrice { get; set; } // Giá thấp nhất
        public decimal? MaxPrice { get; set; } // Giá cao nhất
        public decimal? AveragePrice { get; set; } // Giá trung bình
        public decimal? CurrentPrice { get; set; } // Giá hiện tại (giá gần nhất)
        public decimal TotalValue { get; set; } // Tổng giá trị (quantity * price)
        public decimal TotalOrderAmount { get; set; } // Tổng tiền đặt hàng
        public int POCount { get; set; } // Number of POs containing this product
        public List<POProductDetailViewModel> PODetails { get; set; } = new();
    }

    /// <summary>
    /// PO detail for a specific product
    /// </summary>
    public class POProductDetailViewModel
    {
        public int PO_ID { get; set; }
        public string POCode { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal Value { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.Enums;

namespace Tasin.Website.Models.ViewModels
{
    /// <summary>
    /// View model for invoice information
    /// </summary>
    public class InvoiceViewModel
    {
        /// <summary>
        /// Invoice ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Invoice Code
        /// </summary>
        [Required]
        [Display(Name = "Invoice Code")]
        public string InvoiceCode { get; set; }

        /// <summary>
        /// Purchase Order ID
        /// </summary>
        [Required]
        [Display(Name = "Purchase Order ID")]
        public int PurchaseOrderId { get; set; }

        /// <summary>
        /// Purchase Order Code
        /// </summary>
        [Display(Name = "Purchase Order Code")]
        public string PurchaseOrderCode { get; set; }

        /// <summary>
        /// Invoice Date
        /// </summary>
        [Required]
        [Display(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Due Date
        /// </summary>
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Customer Information
        /// </summary>
        [Display(Name = "Customer")]
        public CustomerInvoiceInfo Customer { get; set; }

        /// <summary>
        /// Company Information
        /// </summary>
        [Display(Name = "Company")]
        public CompanyInvoiceInfo Company { get; set; }

        /// <summary>
        /// Invoice Items
        /// </summary>
        [Display(Name = "Invoice Items")]
        public List<InvoiceItemViewModel> Items { get; set; } = new List<InvoiceItemViewModel>();

        /// <summary>
        /// Subtotal (before tax)
        /// </summary>
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Total Tax Amount
        /// </summary>
        [Display(Name = "Tax Amount")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total Amount (including tax)
        /// </summary>
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Notes
        /// </summary>
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Payment Terms
        /// </summary>
        [Display(Name = "Payment Terms")]
        public string PaymentTerms { get; set; }
    }

    /// <summary>
    /// Customer information for invoice
    /// </summary>
    public class CustomerInvoiceInfo
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneContact { get; set; }
        public string Email { get; set; }
        public string TaxCode { get; set; }
    }

    /// <summary>
    /// Company information for invoice
    /// </summary>
    public class CompanyInvoiceInfo
    {
        public string Name { get; set; } = "CÔNG TY TNHH TASIN";
        public string Address { get; set; } = "Địa chỉ công ty";
        public string Phone { get; set; } = "Số điện thoại";
        public string Email { get; set; } = "Email công ty";
        public string TaxCode { get; set; } = "Mã số thuế";
        public string BankAccount { get; set; } = "Số tài khoản";
        public string BankName { get; set; } = "Tên ngân hàng";
    }

    /// <summary>
    /// Invoice item view model
    /// </summary>
    public class InvoiceItemViewModel
    {
        /// <summary>
        /// Item sequence number
        /// </summary>
        [Display(Name = "STT")]
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Product ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Product Code
        /// </summary>
        [Display(Name = "Product Code")]
        public string ProductCode { get; set; }

        /// <summary>
        /// Product Name
        /// </summary>
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        /// <summary>
        /// Unit
        /// </summary>
        [Display(Name = "Unit")]
        public string Unit { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unit Price
        /// </summary>
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Tax Rate (%)
        /// </summary>
        [Display(Name = "Tax Rate")]
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Amount before tax
        /// </summary>
        [Display(Name = "Amount Before Tax")]
        public decimal AmountBeforeTax { get; set; }

        /// <summary>
        /// Tax Amount
        /// </summary>
        [Display(Name = "Tax Amount")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total Amount (including tax)
        /// </summary>
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Notes
        /// </summary>
        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }
}

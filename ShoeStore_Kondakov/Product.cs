namespace ShoeStore_Kondakov
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.OrderContents = new HashSet<OrderContent>();
        }

        public int Id { get; set; }
        public string Article { get; set; }
        public int NameId { get; set; }
        public string Unit { get; set; }
        public double Price { get; set; }
        public int SupplierId { get; set; }
        public int ManufacturerId { get; set; }
        public int ProductCategoryId { get; set; }
        public int Discount { get; set; }
        public string DiscountText => Discount.ToString() + "%";
        public int Count { get; set; }
        public string Description { get; set; }
        public string Photo { get; set; }

        public bool isDiscount => Discount > 15;
        public bool isOnSale => Discount > 0;
        public double? priceDiscount
        {
            get
            {
                return Price - (Price * Discount / 100.0);
            }
        }
        public bool isQuantity => Count <= 0;

        public string ProductDisplayName
        {
            get
            {
                string title = ProductName != null ? ProductName.Name : (Description ?? "Товар");
                double actualPrice = priceDiscount ?? Price;
                return $"{Article} — {title} ({actualPrice:N2} ₽, на складе: {Count} шт.)";
            }
        }

        public string FullPhotoPath
        {
            get
            {
                string fallback = "/Assets/picture.png";

                if (string.IsNullOrWhiteSpace(Photo))
                {
                    return fallback;
                }

                string candidate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", Photo);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                string candidateAssets = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", Photo);
                if (File.Exists(candidateAssets))
                {
                    return candidateAssets;
                }

                return fallback;
            }
        }

        public virtual Manufacturer Manufacturer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderContent> OrderContents { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }
        public virtual ProductName ProductName { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
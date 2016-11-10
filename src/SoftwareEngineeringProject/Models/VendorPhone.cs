﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoftwareEngineeringProject.Models
{
    public class VendorPhone : WebCrawlerManagedObject
    {
        [Key]
        [Column(Order = 0, TypeName = "varchar(64)")]
        public string VendorID { get; set; }
        [ForeignKey("VendorID")]
        public Vendor Vendor { get; set; }
        [Key]
        [Column(Order = 1, TypeName = "varchar(64)")]
        public string PhoneModelID { get; set; }
        [Key]
        [Column(Order = 2, TypeName = "tinyint")]
        public byte PhoneModelVariantID { get; set; }
        [ForeignKey("PhoneModelID,PhoneModelVariantID")]
        public Phone Phone { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 3, TypeName = "tinyint")]
        public byte PhoneVendorPhoneID { get; set; }
        [Column(Order = 4, TypeName = "varchar(64)")]
        public string CarrierID { get; set; }
        [ForeignKey("CarrierID")]
        public Carrier Carrier { get; set; }
        [Column(Order = 5, TypeName = "varchar(32)")]
        public string PaymentType { get; set; }
        [Column(Order = 6, TypeName = "varchar(16)")]
        public string Price { get; set; }
        [Column(Order = 7, TypeName = "varchar(2083)")]
        public string URL { get; set; }
        [Column(Order = 8, TypeName = "nvarchar(4000)")]
        public string Description { get; set; }
        [Column(Order = 9, TypeName = "datetime2")]
        public override DateTime? LastUpdatedDate { get; set; }
        [NotMapped]
        protected override string[] ColumnNames { get
            {
                return new string[] { nameof(VendorID), nameof(PhoneModelID), nameof(PhoneModelVariantID), nameof(PhoneVendorPhoneID), nameof(CarrierID), nameof(PaymentType), nameof(Price), nameof(URL), nameof(Description), nameof(LastUpdatedDate) };
            }
        }
        [NotMapped]
        protected override object[] Values { get
            {
                return new object[] { VendorID, PhoneModelID, PhoneModelVariantID, PhoneVendorPhoneID, CarrierID, PaymentType, Price, URL, Description, LastUpdatedDate };
            }
        }
        [NotMapped]
        protected override int KeyCount
        {
            get
            {
                return 4;
            }
        }
        
        public string GetColourSpecificImageURL(string firstColour, string vendorID, string initialURL)
        {
            string colour = Phone.Colour;
            // If the phone model has different colour choices and does not match the first colour (the phone colour in the initial URL's page)
            if (firstColour != "N/A" && firstColour != colour) {
                switch (vendorID)
                {
                    case "Bell":
                        if (PhoneModelID.StartsWith("iPhone 7") || PhoneModelID == "iPhone SE")
                        {
                            string pattern = "ac|a|e|il|i|o|u| ";
                            firstColour = Regex.Replace(firstColour, pattern, string.Empty);
                            if (firstColour == "Blk")
                                firstColour = "Mat" + firstColour;
                            if (colour == "Grey")
                                initialURL = initialURL.Replace(firstColour, "Sp" + firstColour);
                            else if (colour == "Gold" && PhoneModelID == "iPhone 7 Plus")
                                colour = "_" + colour;
                            return initialURL.Replace(firstColour, Regex.Replace(colour.Replace("jt", "jet"), pattern, string.Empty));
                        } else if (PhoneModelID.StartsWith("iPhone 6")) {
                            if (firstColour == "Grey")
                                firstColour = "space" + firstColour;
                            return initialURL.Replace(firstColour.ToLower().Replace(" ", string.Empty), colour.Replace(" ", string.Empty));
                        } else if (PhoneModelID.StartsWith("Sony Xperia") || PhoneModelID == "Samsung Galaxy S7")
                        {
                            // Cover a naming inconsistency for the specific phone between different colours
                            if (PhoneModelID == "Sony Xperia X Performance" && firstColour == "Black" && colour == "White") 
                                initialURL = initialURL.Replace("Lrg2", "Lrg2_en");
                            return initialURL.Replace(firstColour, colour);
                        } else {
                            switch (PhoneModelID)
                            {
                                case "Google Pixel":
                                    return initialURL.Replace((firstColour == "Silver" ? "very_" : string.Empty) + firstColour.ToLower(), (colour == "Silver" ? "very_" : string.Empty) + colour.ToLower());
                                case "ALCATEL ONETOUCH Idol X":
                                    return initialURL.Replace("Slate", colour);
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            return initialURL;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}/{2}/{3}/{4}/{5}", GetType().Name, PhoneModelID, PhoneModelVariantID, PhoneVendorPhoneID, CarrierID, PaymentType);
        }
    }
}

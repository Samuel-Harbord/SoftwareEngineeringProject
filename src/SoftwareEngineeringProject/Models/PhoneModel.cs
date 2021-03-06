﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoftwareEngineeringProject.Models
{
    public class PhoneModel : WebCrawlerManagedObject
    {
        [Key]
        [Column(Order = 0, TypeName = "varchar(64)")]
        public string PhoneModelID { get; set; }
        [Column(Order = 1, TypeName = "varchar(64)")]
        public string ManufacturerID { get; set; }
        [ForeignKey("ManufacturerID")]
        public Manufacturer Manufacturer { get; set; }
        [Column(Order = 2, TypeName = "varchar(64)")]
        public string OperatingSystem { get; set; }
        [Column(Order = 3, TypeName = "datetime2")]
        public DateTime? ReleaseDate { get; set; }
        [Column(Order = 4, TypeName = "datetime2")]
        public override DateTime? LastUpdatedDate { get; set; }
        public ICollection<Phone> Phones { get; set; }
        [NotMapped]
        protected override string[] ColumnNames
        {
            get
            {
                return new string[] { nameof(PhoneModelID), nameof(ManufacturerID), nameof(OperatingSystem), nameof(ReleaseDate), nameof(LastUpdatedDate) };
            }
        }
        [NotMapped]
        protected override object[] Values
        {
            get
            {
                return new object[] { PhoneModelID, ManufacturerID, OperatingSystem, ReleaseDate, LastUpdatedDate };
            }
        }
        [NotMapped]
        public string PhoneModelIDWithManufacturer
        {
            get
            {
                return string.Format("{0}{1}", PhoneModelID.ToLower().Contains(ManufacturerID.ToLower()) ? "" : ManufacturerID + " ", PhoneModelID);
            }
        }
        [NotMapped]
        public string FirstImageURL
        {
            get
            {
                string phoneImageURL = "/images/Default.png";
                foreach (Phone phone in Phones)
                {
                    phoneImageURL = phone.GetImageURL();
                    if (phoneImageURL != "/images/Default.png")
                        break;
                }
                return phoneImageURL;
            }
        }

        public void SetOperatingSystem(string os, ICollection<PhoneModel> allPhoneModels, SqlConnection conn)
        {
            if (os != "Android" && os.Contains("Android"))
            {
                Match versionMatch;
                if (!Regex.IsMatch(os, "^Android [0-9x\\.]+( \\([a-z ]+\\))?$", RegexOptions.IgnoreCase))
                {
                    if (os.EndsWith(" Operating System"))
                        os = os.Substring(0, os.IndexOf(" Operating System"));
                    if (os.Contains(" OS; v"))
                        os = os.Replace("OS; ", string.Empty);
                    if (Regex.IsMatch(os, " v[0-9x\\.]+"))
                        os = Regex.Replace(os, " v([0-9x\\.]+)", " $1");
                    if (Regex.IsMatch(os, "^Android( [0-9x\\.]+)? [a-z ]+$", RegexOptions.IgnoreCase))
                        os = string.Format("{0} ({1})", os.Substring(0, os.LastIndexOf(" ")), os.Substring(os.LastIndexOf(" ") + 1));
                    else if (Regex.IsMatch(os, "^Android [a-z ]+ [0-9x\\.]+$", RegexOptions.IgnoreCase))
                        os = string.Format("{0}{1} ({2})", os.Substring(0, os.IndexOf(" ")), os.Substring(os.LastIndexOf(" ")), os.Substring(os.IndexOf(" ") + 1, os.LastIndexOf(" ") - (os.IndexOf(" ") + 1)));
                }
                versionMatch = Regex.Match(os, "^Android ([0-9x\\.]+)? ?(\\([a-z ]+\\))?$", RegexOptions.IgnoreCase);
                string versionNumber = versionMatch.Groups[1].Value;
                string versionName = versionMatch.Groups[2].Value;
                if (versionName == "(M)")
                {
                    versionName = "(Marshmallow)";
                    os = os.Replace("(M)", versionName);
                }
                if (versionNumber != string.Empty)
                {
                    if (versionNumber.IndexOf('.') > -1)
                        versionNumber = versionNumber.Substring(0, versionNumber.IndexOf("."));
                    else
                        os = os.Replace(versionNumber, string.Format("{0}.0", versionNumber));

                    if (versionName != string.Empty)
                        allPhoneModels.Where(pm => pm.OperatingSystem.StartsWith("Android " + versionNumber) && !pm.OperatingSystem.EndsWith(")")).AsParallel()
                            .ForAll(pm => pm.OperatingSystem = string.Format("{0} ({1})", pm.OperatingSystem, versionName));
                    else
                    {
                        PhoneModel phoneModelWithOSVersionName = allPhoneModels.FirstOrDefault(pm => pm.OperatingSystem.StartsWith("Android " + versionNumber) && pm.OperatingSystem.EndsWith(")"));
                        if (phoneModelWithOSVersionName != null)
                            os += " " + phoneModelWithOSVersionName.OperatingSystem.Substring(phoneModelWithOSVersionName.OperatingSystem.LastIndexOf("("));
                    }
                } else if (versionName != string.Empty)
                {
                    if (!Regex.IsMatch(versionName, "\\(OS\\)"))
                    {
                        PhoneModel phoneModelWithOSVersionNumber = allPhoneModels.FirstOrDefault(pm => Regex.IsMatch(pm.OperatingSystem, string.Format("^Android [0-9x\\.]+ \\({0}\\)$", versionName)));
                        if (phoneModelWithOSVersionNumber != null)
                            os = string.Format("{0} {1}", os.Substring(0, os.IndexOf(" ")), phoneModelWithOSVersionNumber.OperatingSystem.Substring(phoneModelWithOSVersionNumber.OperatingSystem.IndexOf(" ") + 1));
                    } else
                        os = os.Substring(0, os.LastIndexOf(" "));
                }
            }
            else if (os == "Apple")
                os = "iOS";

            IEnumerable<string> operatingSystems = allPhoneModels.Select(pm => pm.OperatingSystem).Distinct();

            if (!operatingSystems.Any(pm => pm == os))
            {
                List<OperatingSystemInclusion> inclusions = new List<OperatingSystemInclusion>();
                string version = Regex.Match(os, " ([0-9x\\.]+)?").Groups[1].Value.Replace(".x", string.Empty);
                bool checkVersions = version != string.Empty;
                foreach (string includingOS in operatingSystems)
                {
                    if (includingOS != os)
                    {
                        if (os.Contains(includingOS))
                            inclusions.Add(new OperatingSystemInclusion()
                            {
                                OperatingSystemID = includingOS,
                                IncludedOperatingSystemID = os
                            });
                        else
                        {
                            if (checkVersions)
                            {
                                string includeVersion = Regex.Match(includingOS, " ([0-9x\\.]+)?").Groups[1].Value.Replace(".x", string.Empty);
                                if (includeVersion != string.Empty && version.Contains(includeVersion))
                                {
                                    inclusions.Add(new OperatingSystemInclusion()
                                    {
                                        OperatingSystemID = includingOS,
                                        IncludedOperatingSystemID = os
                                    });
                                }
                            }
                        }
                    }
                }
                inclusions.ForEach(i => i.AddItem(conn));
            }

            OperatingSystem = os;
        }
    }
}

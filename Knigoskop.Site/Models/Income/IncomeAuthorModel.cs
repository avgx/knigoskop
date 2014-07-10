using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Knigoskop.Site.Models
{
  public class IncomeAuthorModel
  {
    public Guid? Id { get; set; }
    public Guid? ImageTempId { get; set; }
    public string Name { get; set; }
    public bool HasImage { get; set; }
    public byte[] Photo { get; set; }
    public DateTime? DeathDate { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Biography { get; set; }
  }
}
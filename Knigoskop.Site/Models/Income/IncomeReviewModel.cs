using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Knigoskop.Site.Models
{
  public class IncomeReviewModel
  {
    public Guid? Id { get; set; }
    public Guid BookId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Rating { get; set; }
  }
}
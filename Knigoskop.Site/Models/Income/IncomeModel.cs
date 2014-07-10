using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Knigoskop.Site.Models
{

  public class IncomeModel
  {
    public Guid? Id { get; set; }
    public IncomeBookModel IncomeBook { get; set; }
    public List<IncomeAuthorModel> IncomeAuthors { get; set; }
    public List<IncomeSerieModel> IncomeSeries { get; set; }
    public List<IncomeGenreModel> IncomeGenres{ get; set; }
    public List<IncomeSimilarBookModel> IncomeSimilarBooks { get; set; }
  }
}
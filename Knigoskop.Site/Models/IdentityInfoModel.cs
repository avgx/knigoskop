using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Knigoskop.Site.Common.Mvc;
using Knigoskop.Site.Localization;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Models
{
  public class IdentityInfoModel : BaseItemModel
  {
    public IdentityInfoModel()
    {
      LinkedProviders = new List<string>();
    }

    [Required(ErrorMessageResourceType = typeof(Text), ErrorMessageResourceName = "FieldRequiredMessage")]
    [Display(ResourceType = typeof(Text), Name = "FirstName")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessageResourceType = typeof(Text), ErrorMessageResourceName = "FieldRequiredMessage")]
    [Display(ResourceType = typeof(Text), Name = "LastName")]    
    public string LastName { get; set; }

    [EmailAddress(ErrorMessageResourceType = typeof(Text), ErrorMessageResourceName = "FieldWrongEmailMessage", ErrorMessage = null)]
    [Required(ErrorMessageResourceType = typeof(Text), ErrorMessageResourceName = "FieldRequiredMessage")]
    [DataType(DataType.EmailAddress)]
    [Display(ResourceType = typeof(Text), Name = "EmailShort")]
    public string Email { get; set; }

    public override string Name
    {
      get
      {
        return FirstName + " " + LastName;
      }
    }

    public DateTime AvatarUpdated { get; set; }

    public IEnumerable<string> LinkedProviders { get; set; }

    public override ItemTypeEnum ItemType
    {
      get { return ItemTypeEnum.User; }
    }
    public int CommentsCount { get; set; }
    public int ReviewsCount { get; set; }
    public int BooksCount { get; set; }
    public int RatingsCount { get; set; }
    public bool IsSubscriber { get; set; }
  }
}
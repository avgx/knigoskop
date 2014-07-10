﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Knigoskop.DataModel
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Entities : DbContext
    {
        public Entities()
            : base("name=Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Author> Authors { get; set; }
        public DbSet<BookContent> BookContents { get; set; }
        public DbSet<BookInSerie> BookInSeries { get; set; }
        public DbSet<BookMedia> BookMedias { get; set; }
        public DbSet<BookRelation> BookRelations { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ExternalResource> ExternalResources { get; set; }
        public DbSet<ExternalUri> ExternalUris { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Serie> Series { get; set; }
        public DbSet<TranslatorsInBook> TranslatorsInBooks { get; set; }
        public DbSet<UserAuthProfile> UserAuthProfiles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserStat> UserStats { get; set; }
        public DbSet<ViewStat> ViewStats { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<Complain> Complains { get; set; }
        public DbSet<OpdsLink> OpdsLinks { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Flag> Flags { get; set; }
        public DbSet<Citation> Citations { get; set; }
        public DbSet<Download> Downloads { get; set; }
    }
}
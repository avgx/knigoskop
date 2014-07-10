using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Knigoskop.DataModel;
using Knigoskop.Site.Models;
using Knigoskop.Site.Models.Shared;
using Knigoskop.Site.Services.Interface;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Ru;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;


namespace Knigoskop.Site.Services
{
    public class SearchService : BasicDataService, ISearchService
    {
        protected SearchStorageModel Storage { get; private set; }

        public void Initialize(SearchStorageModel storage)
        {
            Storage = storage;
        }

        private string EscapeString(string source)
        {
            source = source.Replace(@"\", @"\\");
            source = source.Replace("+", @"\+");
            source = source.Replace("- ", @"\-");
            source = source.Replace("&&", @"\&&");
            source = source.Replace("||", @"\||");
            source = source.Replace("!", @"\!");
            source = source.Replace("(", @"\(");
            source = source.Replace(")", @"\)");
            source = source.Replace("{", @"\{");
            source = source.Replace("}", @"\}");
            source = source.Replace("[", @"\[");
            source = source.Replace("]", @"\]");
            source = source.Replace("^", @"\^");
            source = source.Replace(@"""", @"\""");
            source = source.Replace("~", @"\~");
            source = source.Replace("*", @"\*");
            source = source.Replace("?", @"\?");
            source = source.Replace(":", @"\:");
            return ReplaceSimilars(source);
        }

        private DirectoryInfo GetDirectoryInfo(string directoryName)
        {
            return new DirectoryInfo(string.Format("{0}\\LuceneIndex\\", directoryName));
        }

        private Directory GetLuceneDirectory()
        {
            Directory result = null;
            switch (Storage.StorageType)
            {
                case SearchStorageTypeEnum.FileSystem:
                    DirectoryInfo directoryInfo = GetDirectoryInfo(Storage.DirectoryName);
                    result = FSDirectory.Open(directoryInfo);
                    break;
                case SearchStorageTypeEnum.Azure:
                    break;
            }
            return result;
        }

        private string ReplaceSimilars(string source)
        {
            return source.Replace("Ё", "Е").Replace("ё", "е");
        }

        private void DropDirectory()
        {
            switch (Storage.StorageType)
            {
                case SearchStorageTypeEnum.FileSystem:
                    DirectoryInfo directoryInfo = GetDirectoryInfo(Storage.DirectoryName);
                    if (directoryInfo.Exists)
                        directoryInfo.Delete(true);
                    break;
                case SearchStorageTypeEnum.Azure:
                    break;
            }
        }


        private void CheckDirectoryExists()
        {
            bool directoryExists = false;
            switch (Storage.StorageType)
            {
                case SearchStorageTypeEnum.FileSystem:
                    DirectoryInfo directoryInfo = GetDirectoryInfo(Storage.DirectoryName);
                    directoryExists = directoryInfo.Exists;
                    break;
                case SearchStorageTypeEnum.Azure:
                    break;
            }
            if (!directoryExists)
                CreateIndex();
        }

        public Analyzer GetAnalyzer()
        {
            //var result = new RussianAnalyzer(Version.LUCENE_30);
            var result = new  StandardAnalyzer(Version.LUCENE_30);
            return result;
        }

        public IEnumerable<LuceneResultModel> GetSearchResults(string searchQuery, ItemTypeEnum? itemType = null)
        {
            CheckDirectoryExists();            

            using (Directory directory = GetLuceneDirectory())
            {
                using (Analyzer analyzer = GetAnalyzer())
                {
                    using (IndexReader indexReader = IndexReader.Open(directory, true))
                    {
                        using (Searcher indexSearch = new IndexSearcher(indexReader))
                        {
                            searchQuery = EscapeString(searchQuery.Trim());
                            //string queryText = "";

                            //bool firstIteration = true;
                            //foreach (string word in searchQuery.Trim().Split(' '))
                            //{
                            //    if (firstIteration)
                            //        queryText += word;
                            //    else
                            //        queryText += " AND " + word;
                            //    firstIteration = false;
                            //}
                            
                            searchQuery = EscapeString(searchQuery.Trim());
                            var query = new BooleanQuery();
                            foreach (string word in searchQuery.Trim().Split(' '))
                            {
                                query.Add(new WildcardQuery(new Term("Title", string.Format("*{0}*", word.Trim().ToLower()))), Occur.MUST);
                            }                            

                            //var queryParser = new QueryParser(Version.LUCENE_30, "Title", analyzer);
                            //var query = queryParser.Parse(queryText);                                                        
                            //bQuery.Add(query, Occur.MUST);                             

                            if (itemType != null)
                            {
                                var queryParser = new QueryParser(Version.LUCENE_30, "Type", analyzer);
                                query.Add(queryParser.Parse(Convert.ToString((int)itemType)), Occur.MUST);
                            }

                            TopDocs resultDocs = indexSearch.Search(query, indexReader.MaxDoc);                            

                            return
                                resultDocs.ScoreDocs.Select(a => new { a.Score, a.Doc })
                                          .Select(a => new { a.Score, Doc = indexSearch.Doc(a.Doc) })
                                          .Select(a => new LuceneResultModel
                                          {
                                              Id = new Guid(a.Doc.Get("Id")),
                                              Type = (ItemTypeEnum)Convert.ToInt16(a.Doc.Get("Type")),
                                              Score = a.Score
                                          }).ToList();
                        }
                    }
                }
            }

        }

        public void CreateIndex()
        {
            DropDirectory();

            using (Directory directory = GetLuceneDirectory())
            {
                using (Analyzer analyzer = GetAnalyzer())
                {
                    using (var writer = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.LIMITED))
                    {
                        using (Entities context = GetContext())
                        {
                            var items = context.Authors.Where(a=>a.Books.Any()).OrderBy(ar => ar.Name)
                                               .Select(a => new
                                               {
                                                   Id = a.AuthorId,
                                                   ItemType = (int)ItemTypeEnum.Author,
                                                   Title = a.Name
                                               })
                                               .Union(
                                                   context.Books.OrderBy(br => br.Name)
                                                          .Select(b => new
                                                          {
                                                              Id = b.BookId,
                                                              ItemType = (int)ItemTypeEnum.Book,
                                                              Title = b.Name
                                                          }))
                                               .Union(
                                                   context.Series.OrderBy(br => br.Name)
                                                          .Select(b => new
                                                          {
                                                              Id = b.SerieId,
                                                              ItemType = (int)ItemTypeEnum.Serie,
                                                              Title = b.Name
                                                          }));

                            foreach (var item in items)
                            {
                                var doc = new Document();
                                doc.Add(new Field("Id", item.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                                doc.Add(new Field("Type", Convert.ToString(item.ItemType), Field.Store.YES,
                                                  Field.Index.ANALYZED));
                                doc.Add(new Field("Title", ReplaceSimilars(item.Title), Field.Store.NO, Field.Index.ANALYZED));
                                writer.AddDocument(doc);
                            }
                        }
                        writer.Optimize();
                    }
                }
            }
        }

    }
}
/*
Author          : Application Developer Oleg Gorlov
Description:	: Business Logic controller to access SQL database tables. 
                : Database context is the main class that coordinates Entity Framework functionality .NET Core
Copyright       : DACgroup 
email           : ogorlov@dacgroup.com
Date            : 08/17/2017
Release         : 1.0.0
Comment         : 
*/
using System.Collections.Generic;
using System.Linq;

namespace DAC.LPM.SQLServer
{
    public class SQLServerServices
    {
        private DataDbContext _context;

        public SQLServerServices(DataDbContext context) 
        {
            _context = context;
        }

        public void Add(string sourceName)
        {
            var reviewPage = new ReviewPage { SourceName = sourceName };
            _context.ReviewPages.Add(reviewPage);
            _context.SaveChanges();
        }

        public IEnumerable<ReviewPage> Find(string term) 
        {
            return _context.ReviewPages
                .Where(b => b.SourceName.Contains(term))
                .OrderBy(b => b.SourceName)
                .ToList();
        }

        public IEnumerable<ReviewPage> FindAllReviews(string term) 
        {
            return _context.ReviewPages
                .Where(b => b.SourceName.Contains(term))
                .OrderBy(b => b.SourceName)
                .ToList();
        }
    }
}

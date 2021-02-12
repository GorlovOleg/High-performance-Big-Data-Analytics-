using AFC.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace i4C.BAL.AFC
{
    public interface ICityStateCountryRepository
    {
        Task<IList<CityStateCountries>> GetCityStateCountryList(string SearchValue);
...
using Master.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightBookingController : ControllerBase
    {
        private readonly MasterDBContext _dbContext;

        public FlightBookingController(MasterDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("GetAirlines")]
        public async Task<ActionResult<IEnumerable<Flight>>> GetAirlines()
        {
            var data = await _dbContext.Airlines.ToListAsync();
            return Ok(data);
        }

        [HttpPost("AddorUpdateAirlines")]
        public async Task<ActionResult> AddorUpdateAirlines(Airline airline)
        {
            var data = await _dbContext.Airlines.AsNoTracking().Where(a => a.AirlineId == airline.AirlineId).FirstOrDefaultAsync();
            if (data != null)
            {
                _dbContext.Airlines.Update(airline);
                await _dbContext.SaveChangesAsync();
                return Ok(airline);
            }
            else
            {
                var data1 = _dbContext.Airlines.Add(airline);
                await _dbContext.SaveChangesAsync();
                return Ok(airline);
            }
        }

        [HttpPost("AddorUpdateFlight")]
        public async Task<ActionResult> AddorUpdateFlight(Flight flight)
        {
            var data = await _dbContext.Flights.AsNoTracking().Where(a => a.FlightId == flight.FlightId).FirstOrDefaultAsync();
            if (data != null)
            {
                _dbContext.Flights.Update(flight);
                await _dbContext.SaveChangesAsync();
                return Ok(flight);
            }
            else
            {
                var data1 = _dbContext.Flights.Add(flight);
                await _dbContext.SaveChangesAsync();
                return Ok(flight);
            }
        }

        [HttpGet("GetFlights")]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlights()
        {
            var airlines = await (from airline in _dbContext.Airlines
                                  join flight in _dbContext.Flights on airline.AirlineId equals flight.AirlineId
                                  join departureAirport in _dbContext.Airports on flight.DepartureAirportId equals departureAirport.AirportId
                                  join arrivalAirport in _dbContext.Airports on flight.ArrivalAirportId equals arrivalAirport.AirportId
                                  join departurecity in _dbContext.Cities on departureAirport.CityId equals departurecity.CityId
                                  join arrivalCity in _dbContext.Cities on arrivalAirport.CityId equals arrivalCity.CityId
                                  select new
                                  {
                                      Logo = airline.LogoUrl,
                                      FlightId = flight.FlightId,
                                      FlightNumber = flight.FlightNumber,
                                      AirlineName = airline.AirlineName,
                                      DepartureAirportName = departureAirport.Name,
                                      DepartureCityName = departurecity.Name,
                                      ArrivalAirportName = arrivalAirport.Name,
                                      ArrivalCityName = arrivalCity.Name,
                                      DepartureTime = flight.DepartureTime,
                                      ArrivalTime = flight.ArrivalTime,
                                      Duration = flight.Duration,
                                      EconomyClass = flight.EconomyClass,
                                      BuisnessClass = flight.BusinessClass
                                  }).ToListAsync();


            return Ok(airlines);



        }

        [HttpGet("GetFlightsById")]
        public async Task<ActionResult<Flight>> GetFlightsById(int id)
        {
            var data = await _dbContext.Flights.FindAsync(id);
            return Ok(data);
        }


        [HttpGet("GetCities")]
        public async Task<ActionResult<IEnumerable<Flight>>> GetCities()
        {
            var data = await _dbContext.Cities.ToListAsync();
            return Ok(data);
        }

        [HttpGet("GetAirportsCities")]
        public async Task<ActionResult<IEnumerable<Flight>>> GetAirportsCities()
        {
            var airportCityList = await (from city in _dbContext.Cities
                                         join airport in _dbContext.Airports on city.CityId equals airport.CityId
                                         join departureAirport in _dbContext.Airports on city.CityId equals departureAirport.CityId
                                         join arrivalAirport in _dbContext.Airports on city.CityId equals arrivalAirport.CityId
                                         join departurecity in _dbContext.Cities on departureAirport.CityId equals departurecity.CityId
                                         join arrivalCity in _dbContext.Cities on arrivalAirport.CityId equals arrivalCity.CityId
                                         select new
                                         {
                                             AirportId = airport.AirportId,
                                             CityId = city.CityId,
                                             DepartureAirportName = departureAirport.Name,
                                             DepartureCityName = departurecity.Name,
                                             ArrivalAirportName = arrivalAirport.Name,
                                             ArrivalCityName = arrivalCity.Name,

                                         }).ToListAsync();

            return Ok(airportCityList);
        }
    }
}

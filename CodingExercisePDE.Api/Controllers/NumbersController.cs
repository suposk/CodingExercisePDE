using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodingExercisePDE.Domain;
using CodingExercisePDE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CodingExercisePDE.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NumbersController : ControllerBase
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly ICacheProvider _cacheProvider;        
        private readonly ILogger<NumbersController> _logger;
        const string cacheKey = "NumberDtos";

        public NumbersController(
            ICacheProvider cacheProvider,            
            ILogger<NumbersController> logger)
        {
            _cacheProvider = cacheProvider;            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpGet]
        public List<NumberDto> Get()
        {
            _logger.LogDebug($"{nameof(Get)} Started");

            _semaphore.Wait();
            var dtos = _cacheProvider.GetFromCache<List<NumberDto>>(cacheKey);
            _semaphore.Release();            
            return dtos;
        }

        [HttpPost]
        public async Task<ActionResult<bool>> Post(NumberDto dto)
        {
            if (dto == null || dto.Number < 0)
                return BadRequest();

            _logger.LogDebug($"{nameof(Post)} mesasage number {dto.Number}, Id {dto.Id} received ");

            //#region Test Retry            

            //int x = 3;
            //if (dto.Number >= x)
            //    //throw new Exception($"test exception value {dto.Number}");
            //    return Problem($"test error for value {dto.Number}");

            //#endregion

            _semaphore.Wait();
            var dtos = _cacheProvider.GetFromCache<List<NumberDto>>(cacheKey);
            if (dtos == null)
                dtos = new List<NumberDto>();

            dtos.Add(dto);
            _cacheProvider.SetCache(cacheKey, dtos);
            _semaphore.Release();
            return true;
        }
    }
}

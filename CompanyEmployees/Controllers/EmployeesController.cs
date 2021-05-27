using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers
{
  [Route( "api/companies/{companyId}/employees" )]
  [ApiController]
  public class EmployeesController : ControllerBase
  {
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;

    public EmployeesController( IRepositoryManager repository, ILoggerManager logger, IMapper mapper )
    {
      _repository = repository;
      _logger = logger;
      _mapper = mapper;
    }

    [HttpGet]
    public IActionResult GetEmployeesForCompany( Guid companyId )
    {
      var company = _repository.Company.GetCompany( companyId, false );
      if( company == null )
      {
        _logger.LogInfo( $"Company with id: {companyId} doesn't exist in the database" );
        return NotFound();
      }
      var employeesFromDb = _repository.Employee.GetEmployees( companyId, false );
      var employessDto = _mapper.Map<IEnumerable<EmployeeDto>>( employeesFromDb );
      return Ok( employessDto );
    }

    [HttpGet( "{id}", Name = "GetEmployeeForCompany" )]
    public IActionResult GetEmployeeForCompany( Guid companyId, Guid id )
    {
      var employee = _repository.Employee.GetEmployee( companyId, id, false );
      if( employee == null )
      {
        _logger.LogInfo( $"Employee with id: {id} doesn't exist in the database" );
        return NotFound();
      }
      var employeeDto = _mapper.Map<EmployeeDto>( employee );
      return Ok( employeeDto );
    }

    [HttpPost]
    public IActionResult CreateEmployee( Guid companyId, [FromBody] EmployeeForCreationDto employee )
    {
      var company = _repository.Company.GetCompany( companyId, false );
      if( company == null )
      {
        _logger.LogInfo( $"Company with id {companyId} doesn't exits" );
        return NotFound();
      }

      if( employee == null )
      {
        _logger.LogError( "EmployeeForCreationDto object sent from the client is null" );
        return BadRequest( "EmployeeForCreationDto object is null" );
      }

      var employeeEntity = _mapper.Map<Employee>( employee );
      _repository.Employee.CreateEmployee( companyId, employeeEntity );
      _repository.Save();

      var employeeToReturn = _mapper.Map<EmployeeDto>( employeeEntity );

      return CreatedAtRoute( "GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn );
    }

    [HttpDelete( "{id}" )]
    public IActionResult DeleteEmployee( Guid companyId, Guid id )
    {
      var company = _repository.Company.GetCompany( companyId, false );
      if( company == null )
      {
        _logger.LogInfo( $"Company with id {companyId} doesn't exist in the database" );
        return NotFound();
      }
      var employeeForCompany = _repository.Employee.GetEmployee( companyId, id, false );
      if( employeeForCompany == null )
      {
        _logger.LogError( $"Employee with id {id} doesn't exist in the database" );
        return BadRequest();
      }

      _repository.Employee.DeleteEmployee( employeeForCompany );
      _repository.Save();

      return NoContent();
    }
  }
}
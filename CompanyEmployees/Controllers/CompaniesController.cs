﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers
{
  [Route( "api/companies" )]
  [ApiController]
  public class CompaniesController : ControllerBase
  {
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;

    public CompaniesController( IRepositoryManager repository, ILoggerManager logger, IMapper mapper )
    {
      _repository = repository;
      _logger = logger;
      _mapper = mapper;
    }

    [HttpGet]
    public IActionResult GetCompanies()
    {
      var companies = _repository.Company.GetAllCompanies( false );
      IEnumerable<CompanyDto> companiesDto = _mapper.Map<IEnumerable<CompanyDto>>( companies );
      return Ok( companiesDto );
    }

    [HttpGet( "{id}", Name = "GetCompanyById" )]
    public IActionResult GetCompany( Guid id )
    {
      var company = _repository.Company.GetCompany( id, false );
      if( company == null )
      {
        _logger.LogInfo( $"Company with id: {id} doesn't exist in the database." );
        return NotFound();
      }
      else
      {
        var companyDto = _mapper.Map<CompanyDto>( company );
        return Ok( companyDto );
      }
    }

    [HttpGet( "collection/({ids})", Name = "CompanyCollection" )]
    public IActionResult GetCompanyCollection( IEnumerable<Guid> ids )
    {
      if( ids == null )
      {
        _logger.LogError( "Parameter ids is null" );
        return BadRequest( "Parameter ids is null" );
      }

      var companyEntities = _repository.Company.GetByIds( ids, false );
      if( ids.Count() != companyEntities.Count() )
      {
        _logger.LogError( "Some ids are not valid in a collection" );
        return NotFound();
      }

      var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>( companyEntities );
      return Ok( companiesToReturn );
    }

    [HttpPost]
    public IActionResult CreateCompany( [FromBody] CompanyForCreationDto company )
    {
      if( company == null )
      {
        _logger.LogError( "CompanyForCreationDto object sent from the client is null." );
        return BadRequest( "CompanyForCreationDto object is null" );
      }

      var companyEntity = _mapper.Map<Company>( company );
      _repository.Company.CreateCompany( companyEntity );
      _repository.Save();

      var companyToReturn = _mapper.Map<CompanyDto>( companyEntity );
      return CreatedAtRoute( "GetCompanyById", new { Id = companyToReturn.Id }, companyToReturn );
    }

    [HttpPost]
    public IActionResult CreateCompanyCollection( [FromBody] IEnumerable<CompanyForCreationDto> companies )
    {
      if( companies == null )
      {
        _logger.LogError( "Company Collection is null" );
        return BadRequest( "Companies collection is null" );
      }

      var companyEntities = _mapper.Map<IEnumerable<Company>>( companies );
      foreach( var company in companyEntities )
      {
        _repository.Company.CreateCompany( company );
      }
      _repository.Save();

      var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>( companyEntities );
      var ids = string.Join( ",", companyCollectionToReturn.Select( c => c.Id ) );

      return CreatedAtRoute( "CompanyCollection", new { ids }, companyCollectionToReturn );
    }
  }
}
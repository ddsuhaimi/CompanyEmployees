using System;
using System.Collections.Generic;
using System.Text;
using Entities.Models;

namespace Contracts
{
  public interface IEmployeeRepository
  {
    public IEnumerable<Employee> GetEmployees( Guid companyId, bool trackChanges );

    public Employee GetEmployee( Guid companyId, Guid id, bool trackChanges );

    public void CreateEmployee( Guid companyId, Employee employee );
  }
}
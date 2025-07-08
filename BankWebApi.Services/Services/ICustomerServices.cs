using BankWebApi.Connections.Models;
using BankWebApi.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankWebApi.Services.Services
{
    public interface ICustomerServices
    {
        Task<Client> CreateCustomerAsync(CreateCustomerDto dto);
    }
}

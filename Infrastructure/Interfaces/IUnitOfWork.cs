﻿using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository products { get; }
        IRevokedTokenRepository revokedTokens { get; }
        IRefreshTokenRepository refreshTokens { get; }

        Task<int> CompleteAsync();
    }
}

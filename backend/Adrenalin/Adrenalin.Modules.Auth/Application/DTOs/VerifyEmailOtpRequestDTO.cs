using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Auth.Application.DTOs
{
   public sealed record VerifyEmailOtpRequestDTO(
    string Otp);
}
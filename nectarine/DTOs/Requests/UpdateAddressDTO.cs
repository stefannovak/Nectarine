using System;
using NectarineAPI.DTOs.Generic;

namespace NectarineAPI.DTOs.Requests;

public class UpdateAddressDTO
{
    public UpdateAddressDTO(UserAddressDTO address, Guid previousAddressId)
    {
        Address = address;
        PreviousAddressId = previousAddressId;
    }

    public Guid PreviousAddressId { get; set; }

    public UserAddressDTO Address { get; set; }
}

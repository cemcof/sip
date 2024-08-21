using System.ComponentModel;
using System.Globalization;
using System.Net.Sockets;
// ReSharper disable InconsistentNaming

namespace sip.Utils;

// ReSharper disable once InconsistentNaming
using System;
using System.Collections;


[TypeConverter(typeof(IPAddrConverter))]
public class IPAddr : IEquatable<IPAddr>
{
    private readonly byte[] _addressBytes;
    private readonly int _prefixLength;
    private readonly AddressFamily _addressFamily;

    // Constructor for parsing from string
    public IPAddr(ReadOnlySpan<char> address)
    {
        var slashIndex = address.IndexOf('/');
        ReadOnlySpan<char> ipPart;
        int prefixLength = -1;

        if (slashIndex != -1)
        {
            ipPart = address[..slashIndex];
            var prefixPart = address[(slashIndex + 1)..];

            if (!int.TryParse(prefixPart, out prefixLength))
            {
                throw new FormatException("Invalid IP prefix format.");
            }
        }
        else
        {
            ipPart = address;
        }

        var ipString = ipPart.ToString();

        if (!IPAddress.TryParse(ipString, out var ip))
        {
            throw new FormatException("Invalid IP address format.");
        }

        _addressFamily = ip.AddressFamily;
        _addressBytes = ip.GetAddressBytes();

        // Set default prefix length if not provided
        if (prefixLength == -1)
        {
            _prefixLength = _addressFamily == AddressFamily.InterNetwork ? 32 : 128;
        }
        else
        {
            _prefixLength = prefixLength;
        }
    }
    
    // Constructor for byte array with optional prefix length
    public IPAddr(byte[] addressBytes, int prefixLength = -1)
    {
        _addressBytes = addressBytes;
        _addressFamily = addressBytes.Length == 4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;

        // Set default prefix length if not provided
        if (prefixLength == -1)
        {
            _prefixLength = _addressFamily == AddressFamily.InterNetwork ? 32 : 128;
        }
        else
        {
            _prefixLength = prefixLength;
        }
    }

    // Constructor for Span<byte> with optional prefix length
    public IPAddr(Span<byte> addressSpan, int prefixLength = -1)
        : this(addressSpan.ToArray(), prefixLength) { }

    public bool CheckAgainst(string address)
    {
        try
        {
            var other = new IPAddr(address);
            return CheckAgainst(other);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public bool CheckAgainst(params IPAddr[] targetIps) 
        => targetIps.Any(CheckAgainst);

    public bool CheckAgainst(IPAddr other)
    {
        if (_addressFamily != other._addressFamily)
            return false;
        
        // Create BitArray representations to compare prefixes
        var sourceIpBits = new BitArray(_addressBytes.Reverse().ToArray());
        var targetIpBits = new BitArray(other._addressBytes.Reverse().ToArray());

        if (sourceIpBits.Length < other._prefixLength || sourceIpBits.Length != targetIpBits.Length)
            return false;
        
        // Obviously, bit shifts would be more elegant than this nested for garbage, but this solution
        // is universal enough to even work for IPv6 addresses
        for (var i = targetIpBits.Length - 1; i >= targetIpBits.Length - other._prefixLength; i--)
        {
            if (sourceIpBits[i] != targetIpBits[i])
            {
                return false;
            }
        }

        return true;
    }

    public override string ToString()
    {
        var ip = new IPAddress(_addressBytes);
        return _prefixLength == (_addressFamily == AddressFamily.InterNetwork ? 32 : 128)
            ? ip.ToString()
            : $"{ip}/{_prefixLength}";
    }
    
    public static bool TryParse(string ipStr, [NotNullWhen(true)] out IPAddr? ip)
    {
        try
        {
            ip = new IPAddr(ipStr);
            return true;
        }
        catch (FormatException e)
        {
            ip = null;
            return false;
        }
    }
    
    public bool Equals(IPAddr? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _addressBytes.SequenceEqual(other._addressBytes) && _prefixLength == other._prefixLength && _addressFamily == other._addressFamily;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((IPAddr) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(((IStructuralEquatable)_addressBytes).GetHashCode(EqualityComparer<byte>.Default), _prefixLength, (int) _addressFamily);
    }

    public static bool operator ==(IPAddr? left, IPAddr? right) => Equals(left, right);

    public static bool operator !=(IPAddr? left, IPAddr? right) => !Equals(left, right);
}


public class IPAddrConverter : TypeConverter
{
    // CanConvertFrom method: Determines if the source type can be converted to IPAddr
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;

        return base.CanConvertFrom(context, sourceType);
    }

    // ConvertFrom method: Converts the source type to IPAddr
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string stringValue)
            return new IPAddr(stringValue.AsSpan());

        return base.ConvertFrom(context, culture, value);
    }

    // CanConvertTo method: Determines if the IPAddr can be converted to the target type
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        if (destinationType == typeof(string))
            return true;

        return base.CanConvertTo(context, destinationType);
    }

    // ConvertTo method: Converts the IPAddr to the target type
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is IPAddr ipAddr)
            return ipAddr.ToString();

        return base.ConvertTo(context, culture, value, destinationType);
    }
}

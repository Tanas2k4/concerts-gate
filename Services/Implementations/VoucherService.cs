using Microsoft.EntityFrameworkCore;
using concerts_gate.server.Common.Enums;
using concerts_gate.server.Common.Exceptions;
using concerts_gate.server.Common.Models;
using concerts_gate.server.DTOs.Vouchers;
using concerts_gate.server.Entities;
using concerts_gate.server.Repositories.Interfaces;
using concerts_gate.server.Services.Interfaces;

namespace concerts_gate.server.Services.Implementations;

/// <summary>
/// Implementation of promotional voucher calculation and anti-abuse service.
/// </summary>
public class VoucherService : IVoucherService
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="VoucherService"/>.
    /// </summary>
    public VoucherService(IVoucherRepository voucherRepository, IAuditLogRepository auditLogRepository)
    {
        _voucherRepository = voucherRepository;
        _auditLogRepository = auditLogRepository;
    }

    /// <inheritdoc />
    public async Task<ValidateVoucherResponseDto> ValidateAndCalculateDiscountAsync(
        string code,
        Guid userId,
        decimal orderAmount,
        CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepository.GetByCodeAsync(code, cancellationToken);
        if (voucher == null)
        {
            throw new VoucherException($"Promotional code '{code}' does not exist in the system.");
        }

        if (!voucher.IsActive)
        {
            throw new VoucherException($"Promotional code '{code}' is currently inactive.");
        }

        var now = DateTime.UtcNow;
        if (now < voucher.ValidFrom)
        {
            throw new VoucherException($"Promotional code '{code}' is not yet valid (starts on: {voucher.ValidFrom:yyyy-MM-dd HH:mm} UTC).");
        }

        if (now > voucher.ValidTo)
        {
            throw new VoucherException($"Promotional code '{code}' has expired (expired on: {voucher.ValidTo:yyyy-MM-dd HH:mm} UTC).");
        }

        if (voucher.CurrentUsageCount >= voucher.MaxUsageCount)
        {
            throw new VoucherException($"Promotional code '{code}' has reached its maximum total redemptions.");
        }

        if (orderAmount < voucher.MinOrderAmount)
        {
            throw new VoucherException($"Order subtotal must reach at least {voucher.MinOrderAmount:N0} VND to apply this voucher (Current: {orderAmount:N0} VND).");
        }

        // Validate per-user redemption limit (if userId is supplied)
        if (userId != Guid.Empty)
        {
            var userUsageCount = await _voucherRepository.GetUserUsageCountAsync(voucher.Id, userId, cancellationToken);
            if (userUsageCount >= voucher.MaxUsagePerUser)
            {
                throw new VoucherException($"You have reached the maximum allowed limit of {voucher.MaxUsagePerUser} redemptions for code '{code}'.");
            }
        }

        // Calculate discount deduction
        decimal discount = 0;
        if (voucher.DiscountType == DiscountType.Percentage)
        {
            discount = orderAmount * (voucher.DiscountValue / 100m);
            if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount.Value)
            {
                discount = voucher.MaxDiscountAmount.Value;
            }
        }
        else if (voucher.DiscountType == DiscountType.FixedAmount)
        {
            discount = voucher.DiscountValue;
        }

        // Ensure discount cannot exceed total order amount
        discount = Math.Min(discount, orderAmount);
        var finalAmount = Math.Max(0, orderAmount - discount);

        return new ValidateVoucherResponseDto
        {
            Code = voucher.Code,
            Description = voucher.Description,
            OriginalAmount = orderAmount,
            DiscountAmount = discount,
            FinalAmount = finalAmount
        };
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<VoucherDto>> GetAllVouchersAsync(int pageIndex = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _voucherRepository.GetAll()
            .OrderByDescending(v => v.CreatedAt)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VoucherDto
            {
                Id = v.Id,
                Code = v.Code,
                Description = v.Description,
                DiscountType = v.DiscountType,
                DiscountValue = v.DiscountValue,
                MaxDiscountAmount = v.MaxDiscountAmount,
                MinOrderAmount = v.MinOrderAmount,
                MaxUsageCount = v.MaxUsageCount,
                CurrentUsageCount = v.CurrentUsageCount,
                MaxUsagePerUser = v.MaxUsagePerUser,
                ValidFrom = v.ValidFrom,
                ValidTo = v.ValidTo,
                IsActive = v.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<VoucherDto>(items, totalCount, pageIndex, pageSize);
    }

    /// <inheritdoc />
    public async Task<VoucherDto> CreateVoucherAsync(CreateVoucherDto dto, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        var existing = await _voucherRepository.GetByCodeAsync(code, cancellationToken);
        if (existing != null)
        {
            throw new BadRequestException($"Voucher code '{code}' already exists.");
        }

        if (dto.ValidTo <= dto.ValidFrom)
        {
            throw new BadRequestException("Voucher end date must be after start date.");
        }

        var voucher = new Voucher
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = dto.Description.Trim(),
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            MinOrderAmount = dto.MinOrderAmount,
            MaxUsageCount = dto.MaxUsageCount,
            CurrentUsageCount = 0,
            MaxUsagePerUser = dto.MaxUsagePerUser,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _voucherRepository.AddAsync(voucher, cancellationToken);
        await _voucherRepository.SaveChangesAsync(cancellationToken);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            OperatorId = operatorId,
            Action = "CREATE_VOUCHER",
            TargetEntity = nameof(Voucher),
            TargetId = voucher.Id.ToString(),
            Details = $"Created voucher '{voucher.Code}' ({voucher.DiscountType}: {voucher.DiscountValue}, Max: {voucher.MaxUsageCount} uses).",
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
        await _auditLogRepository.SaveChangesAsync(cancellationToken);

        return new VoucherDto
        {
            Id = voucher.Id,
            Code = voucher.Code,
            Description = voucher.Description,
            DiscountType = voucher.DiscountType,
            DiscountValue = voucher.DiscountValue,
            MaxDiscountAmount = voucher.MaxDiscountAmount,
            MinOrderAmount = voucher.MinOrderAmount,
            MaxUsageCount = voucher.MaxUsageCount,
            CurrentUsageCount = voucher.CurrentUsageCount,
            MaxUsagePerUser = voucher.MaxUsagePerUser,
            ValidFrom = voucher.ValidFrom,
            ValidTo = voucher.ValidTo,
            IsActive = voucher.IsActive
        };
    }

    /// <inheritdoc />
    public async Task<bool> ToggleVoucherStatusAsync(Guid id, bool isActive, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepository.GetByIdAsync(id, cancellationToken);
        if (voucher == null)
        {
            throw new NotFoundException($"Voucher not found with ID: {id}");
        }

        voucher.IsActive = isActive;
        _voucherRepository.Update(voucher);
        await _voucherRepository.SaveChangesAsync(cancellationToken);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            OperatorId = operatorId,
            Action = "TOGGLE_VOUCHER_STATUS",
            TargetEntity = nameof(Voucher),
            TargetId = voucher.Id.ToString(),
            Details = $"Changed status of voucher '{voucher.Code}' to {(isActive ? "Active" : "Inactive")}.",
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
        await _auditLogRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}

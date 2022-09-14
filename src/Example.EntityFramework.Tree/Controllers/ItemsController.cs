using System;
using AutoMapper;
using Example.EntityFramework.Tree.Data;
using Example.EntityFramework.Tree.Entities;
using Example.EntityFramework.Tree.Models;
using Example.EntityFramework.Tree.Options;
using kr.bbon.AspNetCore;
using kr.bbon.AspNetCore.Models;
using kr.bbon.AspNetCore.Mvc;
using kr.bbon.Core;
using kr.bbon.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Example.EntityFramework.Tree.Controllers;

[ApiController]
[ApiVersion(DefaultValues.ApiVersion)]
[Route(DefaultValues.RouteTemplate)]
[Area(DefaultValues.AreaName)]
[Produces("application/json")]
public class ItemsController : ApiControllerBase
{
    public ItemsController(AppDbContext dbContext,
        IMapper mapper,
        IOptionsMonitor<TreeOptions> treeOptionsAccessor,
        ILogger<ItemsController> logger)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
        treeOptions = treeOptionsAccessor.CurrentValue ?? new TreeOptions();
        this.logger = logger;
    }

    /// <summary>
    /// Get all items
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ItemModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllItems(string languageCode = "en")
    {
        var items = await dbContext.Items
            .Include(x => x.Children.OrderBy(child => child.Order))
                .ThenInclude(x => x.Children.OrderBy(child => child.Order))
                    .ThenInclude(x => x.Children.OrderBy(child => child.Order))
            .Where(x => x.ParentId == null)
            .Where(x => x.LanguageCode == languageCode)
            .OrderBy(x => x.Order)
            .Select(x => new ItemModel
            {
                Id = x.Id,
                Name = x.Name,
                Url = x.Url,
                Order = x.Order,
                SubItems = x.Children.OrderBy(c1 => c1.Order).Select(c1 => new ItemModel
                {
                    Id = c1.Id,
                    Name = c1.Name,
                    Url = c1.Url,
                    Order = c1.Order,
                    SubItems = c1.Children.OrderBy(c2 => c2.Order).Select(c2 => new ItemModel
                    {
                        Id = c2.Id,
                        Name = c2.Name,
                        Url = c2.Url,
                        Order = c2.Order,
                        SubItems = c2.Children.OrderBy(c3 => c3.Order).Select(c3 => new ItemModel
                        {
                            Id = c3.Id,
                            Name = c3.Name,
                            Url = c3.Url,
                            Order = c3.Order,
                            SubItems = c3.Children.OrderBy(c4 => c4.Order).Select(c4 => new ItemModel
                            {
                                Id = c4.Id,
                                Name = c4.Name,
                                Url = c4.Url,
                                Order = c4.Order,
                            }).ToList(),
                        }).ToList()
                    }).ToList(),
                }).ToList(),
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Get item information
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ItemModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseModel<ErrorModel>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItem([FromRoute] Guid id)
    {
        var item = await dbContext.Items
            .Where(x => x.Id == id)
            .OrderBy(x => x.Order)
            .Select(x => new ItemModel
            {
                Id = x.Id,
                Name = x.Name,
                Url = x.Url,
                Order = x.Order,
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (item == null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Item does not find");
        }

        return Ok(item);
    }

    /// <summary>
    /// Get sub items
    /// </summary>
    /// <param name="parentId"></param>
    /// <returns></returns>
    [HttpGet("{parentId:guid}/subitems")]
    [ProducesResponseType(typeof(IEnumerable<ItemModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubItems([FromRoute] Guid parentId)
    {
        var items = await dbContext.Items
            .Include(x => x.Children.OrderBy(child => child.Order))
                .ThenInclude(x => x.Children.OrderBy(child => child.Order))
                    .ThenInclude(x => x.Children.OrderBy(child => child.Order))
                        .ThenInclude(x => x.Children.OrderBy(child => child.Order))
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.Order)
            .Select(x => new ItemModel
            {
                Id = x.Id,
                Name = x.Name,
                Url = x.Url,
                Order = x.Order,
                SubItems = x.Children.OrderBy(c1 => c1.Order).Select(c1 => new ItemModel
                {
                    Id = c1.Id,
                    Name = c1.Name,
                    Url = c1.Url,
                    Order = c1.Order,
                    SubItems = c1.Children.OrderBy(c2 => c2.Order).Select(c2 => new ItemModel
                    {
                        Id = c2.Id,
                        Name = c2.Name,
                        Url = c2.Url,
                        Order = c2.Order,
                        SubItems = c2.Children.OrderBy(c3 => c3.Order).Select(c3 => new ItemModel
                        {
                            Id = c3.Id,
                            Name = c3.Name,
                            Url = c3.Url,
                            Order = c3.Order,
                            SubItems = c3.Children.OrderBy(c4 => c4.Order).Select(c4 => new ItemModel
                            {
                                Id = c4.Id,
                                Name = c4.Name,
                                Url = c4.Url,
                                Order = c4.Order,
                            }).ToList(),
                        }).ToList()
                    }).ToList(),
                }).ToList(),
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Add item
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    [HttpPost]
    [ProducesResponseType(typeof(ItemModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponseModel<ErrorModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseModel<ErrorModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseModel<ErrorModel>), StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(typeof(ApiResponseModel<ErrorModel>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddItem([FromBody] AddItemModel model)
    {
        Item? parentItem = null;
        Item? addedEntity = null;

        if (!ModelState.IsValid)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Request body is invalid");
        }

        using (var transaction = dbContext.Database.BeginTransaction())
        {
            try
            {
                if (model.ParentId.HasValue)
                {
                    parentItem = await dbContext.Items
                        .Where(x => x.Id == model.ParentId.Value)
                        .FirstOrDefaultAsync();

                    if (parentItem == null)
                    {
                        throw new ApiException(StatusCodes.Status404NotFound, "Parent item does not exists");
                    }
                }
                else
                {
                    var topLevelItems = await dbContext.Items
                        .Where(x => x.ParentId == null && x.LanguageCode == model.LanguageCode)
                        .CountAsync();

                    if (topLevelItems >= treeOptions.TopLevelItemsCount)
                    {
                        throw new ApiException(StatusCodes.Status404NotFound, $"Top level items must less than {treeOptions.TopLevelItemsCount + 1} items");
                    }
                }

                var parentItemId = parentItem?.Id;
                var languageCode = parentItem?.LanguageCode ?? model.LanguageCode;
                int? order = null;

                if (model.Order == 0)
                {
                    var subItems = dbContext.Items
                        .Where(x => x.ParentId == parentItemId);

                    if (subItems.Any())
                    {
                        order = subItems.Max(x => x.Order);
                    }
                    else
                    {
                        order = 1;
                    }
                }

                var newItem = new Item
                {
                    Id = Guid.NewGuid(),
                    LanguageCode = languageCode,
                    Name = model.Name,
                    Url = model.Url,
                    Order = order.HasValue ? order.Value : model.Order,
                    Level = (parentItem?.Level ?? 0) + 1,
                    ParentId = parentItemId,
                };

                if (newItem.Level > treeOptions.MaxLevel)
                {
                    throw new ApiException(StatusCodes.Status406NotAcceptable, $"Item depth must be less than {treeOptions.MaxLevel + 1}");
                }

                var added = dbContext.Items.Add(newItem);

                addedEntity = added.Entity;

                await dbContext.SaveChangesAsync();

                var sibilings = dbContext.Items
                    .Where(x => x.ParentId == parentItemId)
                    .Where(x => x.LanguageCode == languageCode)
                    .Where(x => x.Id != addedEntity.Id)
                    .Where(x => x.Order > addedEntity.Order);

                foreach (var sibiling in sibilings)
                {
                    if (sibiling.Order < addedEntity.Order) { continue; }
                    if (sibiling.Id == addedEntity.Id) { continue; }

                    sibiling.Order += 1;
                }

                await dbContext.SaveChangesAsync();

                var reorderCandidate = dbContext.Items
                    .Where(x => x.ParentId == parentItemId && x.LanguageCode == languageCode)
                    .OrderBy(x => x.Order);

                var reorder = 1;
                foreach (var item in reorderCandidate)
                {
                    item.Order = reorder;
                    reorder += 1;
                }

                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, ex.Message);

                await transaction.RollbackAsync();

                throw;
            }
        }

        var addedItemModel = new ItemModel
        {
            Id = addedEntity.Id,
            Name = addedEntity.Name,
            Url = addedEntity.Url,
            Order = addedEntity.Order,
        };

        return Created($"items/{addedEntity.Id}", addedItemModel);
    }

    /// <summary>
    /// Update item
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    [HttpPut("{id:Guid}")]
    [ProducesResponseType(typeof(ItemModel), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiResponseModel<ErrorModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseModel<ErrorModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseModel<ErrorModel>), StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(typeof(ApiResponseModel<ErrorModel>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateItemModel model)
    {
        Item? parentItem = null;
        Item? updateItem = null;

        if (!ModelState.IsValid)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Request body is invalid");
        }

        model.Id = id;

        using (var transaction = dbContext.Database.BeginTransaction())
        {
            try
            {
                if (model.ParentId.HasValue)
                {
                    parentItem = await dbContext.Items
                        .Where(x => x.Id == model.ParentId.Value)
                        .FirstOrDefaultAsync();

                    if (parentItem == null)
                    {
                        throw new ApiException(StatusCodes.Status404NotFound, "Parent item does not exists");
                    }
                }
                else
                {
                    var topLevelItems = await dbContext.Items
                        .Where(x => x.ParentId == null && x.LanguageCode == model.LanguageCode)
                        .CountAsync();

                    if (topLevelItems >= treeOptions.TopLevelItemsCount)
                    {
                        throw new ApiException(StatusCodes.Status404NotFound, $"Top level items must less than {treeOptions.TopLevelItemsCount + 1} items");
                    }
                }

                var parentItemId = parentItem?.Id;
                Guid? previousParentId = null;
                var languageCode = parentItem?.LanguageCode ?? model.LanguageCode;
                int? order = null;

                updateItem = await dbContext.Items
                    .Where(x => x.Id == model.Id).FirstOrDefaultAsync();

                if (updateItem == null)
                {
                    throw new ApiException(StatusCodes.Status404NotFound);
                }

                if (updateItem.ParentId != parentItemId)
                {
                    previousParentId = updateItem.ParentId;
                }

                if (model.Order == 0)
                {
                    var subItems = dbContext.Items
                        .Where(x => x.ParentId == parentItemId);

                    if (subItems.Any())
                    {
                        order = subItems.Max(x => x.Order);
                    }
                    else
                    {
                        order = 1;
                    }
                }

                updateItem.LanguageCode = languageCode;
                updateItem.Name = model.Name;
                updateItem.Url = model.Url;
                updateItem.Order = order.HasValue ? order.Value : model.Order;
                updateItem.Level = (parentItem?.Level ?? 0) + 1;
                updateItem.ParentId = parentItemId;


                if (updateItem.Level > treeOptions.MaxLevel)
                {
                    throw new ApiException(StatusCodes.Status406NotAcceptable, $"Item depth must be less than {treeOptions.MaxLevel + 1}");
                }

                await dbContext.SaveChangesAsync();

                var siblings = dbContext.Items
                    .Where(x => x.ParentId == parentItemId)
                    .Where(x => x.LanguageCode == languageCode)
                    .Where(x => x.Id != updateItem.Id)
                    .Where(x => x.Order > updateItem.Order);

                foreach (var sibling in siblings)
                {
                    if (sibling.Order < updateItem.Order) { continue; }
                    if (sibling.Id == updateItem.Id) { continue; }

                    sibling.Order += 1;
                }

                await dbContext.SaveChangesAsync();

                var reorderCandidate = dbContext.Items
                    .Where(x => x.ParentId == parentItemId && x.LanguageCode == languageCode)
                    .OrderBy(x => x.Order);

                var reorder = 1;
                foreach (var item in reorderCandidate)
                {
                    item.Order = reorder;
                    reorder += 1;
                }

                await dbContext.SaveChangesAsync();

                if (previousParentId.HasValue)
                {
                    // parent item changed
                    var reorderPreviousSiblings = dbContext.Items
                        .Where(x => x.ParentId == previousParentId.Value && x.LanguageCode == languageCode)
                        .OrderBy(x => x.Order);

                    reorder = 1;
                    foreach (var item in reorderCandidate)
                    {
                        item.Order = reorder;
                        reorder += 1;
                    }

                    await dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, ex.Message);

                await transaction.RollbackAsync();

                throw;
            }
        }

        var updatedModel = new ItemModel
        {
            Id = updateItem.Id,
            Name = updateItem.Name,
            Url = updateItem.Url,
            Order = updateItem.Order,
        };

        return Accepted($"items/{updateItem.Id}", updatedModel);
    }

    /// <summary>
    /// Delete item
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiResponseModel<ErrorModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseModel<ErrorModel>), StatusCodes.Status405MethodNotAllowed)]
    [ProducesResponseType(typeof(ApiResponseModel<ErrorModel>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        using (var transaction = dbContext.Database.BeginTransaction())
        {
            var deleteCandidate = await dbContext.Items
                .Include(x => x.Children)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (deleteCandidate == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Item does not find");
            }

            if (deleteCandidate.Children.Any())
            {
                throw new ApiException(StatusCodes.Status405MethodNotAllowed, "Item that has children items does not allow delete");
            }

            var parentId = deleteCandidate.ParentId;
            var languageCode = deleteCandidate.LanguageCode;

            try
            {
                dbContext.Items.Remove(deleteCandidate);

                await dbContext.SaveChangesAsync();

                if (parentId.HasValue)
                {
                    var reorderCandidate = dbContext.Items
                       .Where(x => x.ParentId == parentId && x.LanguageCode == languageCode)
                       .OrderBy(x => x.Order);

                    var order = 1;
                    foreach (var item in reorderCandidate)
                    {
                        item.Order = order;
                        order += 1;
                    }

                    await dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, ex.Message);

                await transaction.RollbackAsync();

                throw;
            }

            return Accepted();
        }
    }

    private readonly AppDbContext dbContext;
    private readonly IMapper mapper;
    private readonly TreeOptions treeOptions;
    private readonly ILogger logger;
}


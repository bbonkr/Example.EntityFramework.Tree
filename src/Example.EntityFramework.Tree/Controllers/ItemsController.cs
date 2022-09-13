using System;
using AutoMapper;
using Example.EntityFramework.Tree.Data;
using Example.EntityFramework.Tree.Entities;
using Example.EntityFramework.Tree.Models;
using kr.bbon.AspNetCore;
using kr.bbon.AspNetCore.Mvc;
using kr.bbon.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Example.EntityFramework.Tree.Controllers;

[ApiController]
[ApiVersion(DefaultValues.ApiVersion)]
[Route(DefaultValues.RouteTemplate)]
[Area(DefaultValues.AreaName)]
[Produces("application/json")]
public class ItemsController : ApiControllerBase
{
    private const int MAX_LEVEL = 3;
    private const int TOP_LEVEL_ITEM_LIMIT = 5;

    public ItemsController(AppDbContext dbContext, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
    }

    /// <summary>
    /// Get all root items
    /// </summary>
    /// <returns></returns>
    [HttpGet]
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
                Order = x.Order,
                Children = x.Children.Select(c1 => new ItemModel
                {
                    Id = c1.Id,
                    Name = c1.Name,
                    Order = c1.Order,
                    Children = c1.Children.Select(c2 => new ItemModel
                    {
                        Id = c2.Id,
                        Name = c2.Name,
                        Order = c2.Order,
                        Children = c2.Children.Select(c3 => new ItemModel
                        {
                            Id = c3.Id,
                            Name = c3.Name,
                            Order = c3.Order,
                            //Children = c3.Children.Select(c4 => new ItemModel
                            //{
                            //    Id = c4.Id,
                            //    Name = c4.Name,
                            //    Order = c4.Order,
                            //}).ToList(),
                        }).ToList()
                    }).ToList(),
                }).ToList(),

            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Get items
    /// </summary>
    /// <param name="parentId"></param>
    /// <param name="languageCode"></param>
    /// <returns></returns>
    [HttpGet("{parentId}")]
    public async Task<IActionResult> GetItem([FromRoute] Guid parentId, string languageCode = "en")
    {
        var items = await dbContext.Items
            .Include(x => x.Children.OrderBy(child => child.Order))
                .ThenInclude(x => x.Children.OrderBy(child => child.Order))
                    .ThenInclude(x => x.Children.OrderBy(child => child.Order))
            .Where(x => x.ParentId == parentId)
            .Where(x => x.LanguageCode == languageCode)
            .OrderBy(x => x.Order)
            .Select(x => new ItemModel
            {
                Id = x.Id,
                Name = x.Name,
                Order = x.Order,
                Children = x.Children.Select(c1 => new ItemModel
                {
                    Id = c1.Id,
                    Name = c1.Name,
                    Order = c1.Order,
                    Children = c1.Children.Select(c2 => new ItemModel
                    {
                        Id = c2.Id,
                        Name = c2.Name,
                        Order = c2.Order,
                        Children = c2.Children.Select(c3 => new ItemModel
                        {
                            Id = c3.Id,
                            Name = c3.Name,
                            Order = c3.Order,
                            //Children = c3.Children.Select(c4 => new ItemModel
                            //{
                            //    Id = c4.Id,
                            //    Name = c4.Name,
                            //    Order = c4.Order,
                            //}).ToList(),
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

                    if (topLevelItems >= TOP_LEVEL_ITEM_LIMIT)
                    {
                        throw new ApiException(StatusCodes.Status404NotFound, $"Top level items must less than {TOP_LEVEL_ITEM_LIMIT + 1} items");
                    }
                }

                var parentItemId = parentItem?.Id;
                var languageCode = parentItem?.LanguageCode ?? model.LanguageCode;

                var newItem = new Item
                {
                    Id = Guid.NewGuid(),
                    LanguageCode = languageCode,
                    Name = model.Name,
                    Order = model.Order,
                    Level = (parentItem?.Level ?? 0) + 1,
                    ParentId = parentItemId,
                };

                if (newItem.Level > MAX_LEVEL)
                {
                    throw new ApiException(StatusCodes.Status406NotAcceptable, $"Item depth must be less than {MAX_LEVEL + 1}");
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

                var order = 1;
                foreach (var item in reorderCandidate)
                {
                    item.Order = order;
                    order += 1;
                }

                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {

                await transaction.RollbackAsync();

                throw;
            }
        }

        var addedItemModel = new ItemModel
        {
            Id = addedEntity.Id,
            Name = addedEntity.Name,
            Order = addedEntity.Order,
        };

        return Created($"items/{addedEntity.Id}", addedItemModel);
    }

    /// <summary>
    /// Update item
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    [HttpPatch("{id:Guid}")]
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

                    if (topLevelItems >= TOP_LEVEL_ITEM_LIMIT)
                    {
                        throw new ApiException(StatusCodes.Status404NotFound, $"Top level items must less than {TOP_LEVEL_ITEM_LIMIT + 1} items");
                    }
                }

                var parentItemId = parentItem?.Id;
                var languageCode = parentItem?.LanguageCode ?? model.LanguageCode;

                updateItem = await dbContext.Items.Where(x => x.Id == model.Id).FirstOrDefaultAsync();

                if (updateItem == null)
                {
                    throw new ApiException(StatusCodes.Status404NotFound);
                }

                updateItem.LanguageCode = languageCode;
                updateItem.Name = model.Name;
                updateItem.Order = model.Order;
                updateItem.Level = (parentItem?.Level ?? 0) + 1;
                updateItem.ParentId = parentItemId;


                if (updateItem.Level > MAX_LEVEL)
                {
                    throw new ApiException(StatusCodes.Status406NotAcceptable, $"Item depth must be less than {MAX_LEVEL + 1}");
                }

                await dbContext.SaveChangesAsync();

                var sibilings = dbContext.Items
                    .Where(x => x.ParentId == parentItemId)
                    .Where(x => x.LanguageCode == languageCode)
                    .Where(x => x.Id != updateItem.Id)
                    .Where(x => x.Order > updateItem.Order);

                foreach (var sibiling in sibilings)
                {
                    if (sibiling.Order < updateItem.Order) { continue; }
                    if (sibiling.Id == updateItem.Id) { continue; }

                    sibiling.Order += 1;
                }

                await dbContext.SaveChangesAsync();

                var reorderCandidate = dbContext.Items
                    .Where(x => x.ParentId == parentItemId && x.LanguageCode == languageCode)
                    .OrderBy(x => x.Order);

                var order = 1;
                foreach (var item in reorderCandidate)
                {
                    item.Order = order;
                    order += 1;
                }

                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {

                await transaction.RollbackAsync();

                throw;
            }
        }

        var updatedModel = new ItemModel
        {
            Id = updateItem.Id,
            Name = updateItem.Name,
            Order = updateItem.Order,
        };

        return Accepted($"items/{updateItem.Id}", updatedModel);

    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleteCandidate = await dbContext.Items
            .Include(x => x.Children)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (deleteCandidate == null)
        {
            throw new ApiException(StatusCodes.Status404NotFound);
        }

        if (deleteCandidate.Children.Any())
        {
            throw new ApiException(StatusCodes.Status405MethodNotAllowed, "Item that has children items does not allow delete");
        }

        dbContext.Items.Remove(deleteCandidate);

        await dbContext.SaveChangesAsync();

        return Accepted();
    }

    private readonly AppDbContext dbContext;
    private readonly IMapper mapper;
}


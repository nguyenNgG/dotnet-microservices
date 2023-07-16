using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;

namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        private static readonly List<ItemDto> items =
            new()
            {
                new ItemDto(
                    Guid.NewGuid(),
                    "Potion",
                    "Restores a small amount of health",
                    5M,
                    DateTimeOffset.UtcNow
                ),
                new ItemDto(Guid.NewGuid(), "Antidote", "Cures poison", 7M, DateTimeOffset.UtcNow),
                new ItemDto(
                    Guid.NewGuid(),
                    "Bronze Sword",
                    "Deals a small amount of damage",
                    20M,
                    DateTimeOffset.UtcNow
                )
            };

        // GET /items
        [HttpGet]
        public IEnumerable<ItemDto> Get()
        {
            return items;
        }

        // GET /items/{id}
        [HttpGet("{id}")]
        public ActionResult<ItemDto> GetById(Guid id)
        {
            var item = items.Where(item => item.Id == id).SingleOrDefault();

            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        // POST /items
        [HttpPost]
        public ActionResult<ItemDto> Post(CreateItemDto createItemDto)
        {
            var item = new ItemDto(
                Guid.NewGuid(),
                createItemDto.Name,
                createItemDto.Description,
                createItemDto.Price,
                DateTimeOffset.UtcNow
            );

            items.Add(item);

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        // PUT /items/{id}
        [HttpPut("{id}")]
        public IActionResult Put(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem = items.Where(item => item.Id == id).SingleOrDefault();

            if (existingItem == null)
            {
                return NotFound();
            }

            var updatedItem = existingItem! with
            {
                Name = updateItemDto.Name,
                Description = updateItemDto.Description,
                Price = updateItemDto.Price
            };

            var index = items.FindIndex(existingItem => existingItem.Id == id);
            items[index] = updatedItem;

            return NoContent();
        }

        // DELETE /items/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var index = items.FindIndex(existingItem => existingItem.Id == id);

            if (index < 0)
            {
                return NotFound();
            }

            items.RemoveAt(index);

            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using client.Models;
using client.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace client.Controllers
{
    [Route("Books")]
    public class BooksController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:5153/odata";

        public BooksController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/books");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var oDataResponse = JsonConvert.DeserializeObject<ODataResponse<Book>>(jsonData);
                return View(oDataResponse.Value);
            }

            return View(new List<Book>());
        }

        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/books({id})?$expand=Location,Press");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var book = JsonConvert.DeserializeObject<Book>(jsonData);
                return View(book);
            }

            return NotFound();
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                var jsonData = JsonConvert.SerializeObject(book);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_baseUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(book);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var bookResponse = await _httpClient.GetAsync($"{_baseUrl}/books({id})");
            if (bookResponse.IsSuccessStatusCode)
            {
                var jsonData = await bookResponse.Content.ReadAsStringAsync();
                var book = JsonConvert.DeserializeObject<Book>(jsonData);

                var pressResponse = await _httpClient.GetAsync($"{_baseUrl}/presses");
                var presses = new List<SelectListItem>();
                if (pressResponse.IsSuccessStatusCode)
                {
                    var pressData = await pressResponse.Content.ReadAsStringAsync();
                    presses = JsonConvert.DeserializeObject<ODataResponse<Press>>(pressData)
                            .Value.Select(p => new SelectListItem
                            {
                                Value = p.Id.ToString(),
                                Text = p.Name
                            }).ToList();
                }

                var addressResponse = await _httpClient.GetAsync($"{_baseUrl}/addresses");
                var addresses = new List<SelectListItem>();
                if (addressResponse.IsSuccessStatusCode)
                {
                    var addressData = await addressResponse.Content.ReadAsStringAsync();
                    addresses = JsonConvert.DeserializeObject<ODataResponse<Address>>(addressData)
                                .Value.Select(a => new SelectListItem
                                {
                                    Value = a.Id.ToString(),
                                    Text = $"{a.City}, {a.Street}"
                                }).ToList();
                }

                var editDto = new EditBookDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    PressId = book.PressId,
                    AddressId = book.LocationId,
                    Presses = presses,
                    Addresses = addresses
                };

                return View(editDto);
            }

            return NotFound();
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditBookDto dto)
        {
            Console.WriteLine("Editing book with ID: " + id);

            if (ModelState.IsValid)
            {
                Console.WriteLine("Model state is valid. Book data: " + JsonConvert.SerializeObject(dto));

                var jsonData = new Dictionary<string, object>
                {
                    { "Id", dto.Id }
                };

                if (!string.IsNullOrEmpty(dto.Title))
                    jsonData["Title"] = dto.Title;

                if (!string.IsNullOrEmpty(dto.Author))
                    jsonData["Author"] = dto.Author;

                if (dto.PressId.HasValue)
                    jsonData["PressId"] = dto.PressId.Value;

                if (dto.AddressId.HasValue)
                    jsonData["LocationId"] = dto.AddressId.Value;

                var jsonString = JsonConvert.SerializeObject(jsonData);

                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_baseUrl}/books({id})")
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(requestMessage);

                Console.WriteLine("Response status code: " + response.StatusCode);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Book updated successfully.");
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Error response: " + errorContent);
                }
            }
            else
            {
                Console.WriteLine("Model state is invalid. Errors: " + JsonConvert.SerializeObject(ModelState));
            }

            dto.Presses = await GetPresses();
            dto.Addresses = await GetAddresses();
            return View(dto);
        }

        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/books({id})?$expand=Location,Press");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var book = JsonConvert.DeserializeObject<Book>(jsonData);
                return View(book);
            }

            return NotFound();
        }

        [HttpPost("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Console.WriteLine("HERE");
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/books({id})");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            return NotFound();
        }

        private async Task<IEnumerable<SelectListItem>> GetPresses()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/presses");
            var presses = new List<SelectListItem>();
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                presses = JsonConvert.DeserializeObject<ODataResponse<Press>>(data)
                        .Value.Select(p => new SelectListItem
                        {
                            Value = p.Id.ToString(),
                            Text = p.Name
                        }).ToList();
            }
            return presses;
        }

        private async Task<IEnumerable<SelectListItem>> GetAddresses()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/addresses");
            var addresses = new List<SelectListItem>();
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                addresses = JsonConvert.DeserializeObject<ODataResponse<Address>>(data)
                        .Value.Select(a => new SelectListItem
                        {
                            Value = a.Id.ToString(),
                            Text = $"{a.City}, {a.Street}"
                        }).ToList();
            }
            return addresses;
        }
    }
}
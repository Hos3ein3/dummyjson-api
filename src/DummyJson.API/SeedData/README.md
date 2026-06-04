# SeedData Directory

Place DummyJSON seed files here. The DataSeeder reads from these files on first startup
when `SeedDatabase: true` is set in `appsettings.Development.json`.

## Expected Files

| File | Contents |
|---|---|
| `products.json` | Download from https://dummyjson.com/products?limit=200 |
| `users.json` | Download from https://dummyjson.com/users?limit=200 |
| `posts.json` | Download from https://dummyjson.com/posts?limit=200 |
| `todos.json` | Download from https://dummyjson.com/todos?limit=200 |
| `carts.json` | Download from https://dummyjson.com/carts?limit=100 |

## Quick Download (macOS/Linux)

```bash
cd SeedData
curl -o products.json "https://dummyjson.com/products?limit=200"
curl -o users.json "https://dummyjson.com/users?limit=200"
curl -o posts.json "https://dummyjson.com/posts?limit=200"
curl -o todos.json "https://dummyjson.com/todos?limit=200"
curl -o carts.json "https://dummyjson.com/carts?limit=100"
```

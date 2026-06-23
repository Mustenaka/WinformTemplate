# WebApi REST Contract

This contract is the source of truth for P2 `WebApi*Repository` implementations. The backend is not implemented in P2.

## Base Rules

- Base URL comes from `config.json` -> `WebApi.BaseUrl`, for example `https://localhost:5001`.
- All repository URLs below are relative to the base URL.
- Authentication is reserved for a later backend phase. Clients may send `Authorization: Bearer <token>` when available, but P2 repositories do not require a token.
- JSON uses `application/json; charset=utf-8`.
- Date/time query values use ISO 8601, for example `2026-06-23T08:00:00.0000000Z`.

## Response Envelope

Every endpoint returns `ApiResponse<T>`:

```json
{
  "success": true,
  "message": "OK",
  "data": {},
  "errorCode": null,
  "isTransportError": false,
  "timestamp": "2026-06-23T08:00:00"
}
```

Backend business misses use `success: false` with `isTransportError: false`; repositories convert these to `null`, empty results, `false`, or `0` according to the method contract. Transport failures such as connection refused, DNS failure, and timeout are created by `WebApiClient` as `isTransportError: true`; repositories throw `DataSourceUnavailableException`.

## Paging Query

Paged `QueryAsync` maps to:

```http
GET /api/{Module}/{resource}?page=1&pageSize=20&keyword=abc&sortBy=Name&desc=false&filters.status=0
```

Response data:

```json
{
  "items": [],
  "total": 0
}
```

`filters.{name}` values are strings. Repositories never send `Expression<Func<...>>`.

## Standard CRUD

Each resource supports the same CRUD shape:

| Operation | Method and URL | Request Body | Response `data` |
| --- | --- | --- | --- |
| GetById | `GET /api/{Module}/{resource}/{id}` | none | entity or `null` |
| Query | `GET /api/{Module}/{resource}?page=1&pageSize=20&keyword=&sortBy=&desc=false&filters.x=y` | none | `PagedResult<TEntity>` |
| Add | `POST /api/{Module}/{resource}` | entity JSON | created entity |
| Update | `PUT /api/{Module}/{resource}/{id}` | entity JSON | `true` or `false` |
| Delete | `DELETE /api/{Module}/{resource}/{id}` | none | `true` or `false` |

Example add:

```http
POST /api/Template/products
Content-Type: application/json

{ "name": "Keyboard", "code": "EL-KEY-001", "categoryId": 1, "price": 129.00, "stock": 50, "status": 0 }
```

Example boolean response:

```json
{ "success": true, "message": "OK", "data": true, "errorCode": null, "isTransportError": false, "timestamp": "2026-06-23T08:00:00" }
```

## Resources

| Module | Resource | Entity | Repository |
| --- | --- | --- | --- |
| Sys | `accounts` | `SysAccountModel` | `ApiSysAccountRepository` |
| Sys | `menus` | `SysMenuModel` | `ApiSysMenuRepository` |
| Sys | `roles` | `SysRoleModel` | `ApiSysRoleRepository` |
| Sys | `params` | `SysParamModel` | `ApiSysParamRepository` |
| Sys | `role-auths` | `SysRoleAuthModel` | `ApiSysRoleAuthRepository` |
| Template | `products` | `ProductModel` | `ApiProductRepository` |
| Template | `categories` | `CategoryModel` | `ApiCategoryRepository` |
| Template | `import-records` | `ImportRecordModel` | `ApiImportRecordRepository` |

`role-auths` standard CRUD uses composite IDs formatted as `{roleId}:{menuId}` for `GetById`, `Update`, and `Delete`.

## Sys Named Endpoints

### Accounts

| Method and URL | Request Body | Response `data` |
| --- | --- | --- |
| `GET /api/Sys/accounts/by-username/{username}` | none | `SysAccountModel` or `null` |
| `POST /api/Sys/accounts/{id}/freeze` | `{}` | `true` or `false` |
| `POST /api/Sys/accounts/{id}/unfreeze` | `{}` | `true` or `false` |

### Menus

| Method and URL | Request Body | Response `data` |
| --- | --- | --- |
| `GET /api/Sys/menus/active` | none | `SysMenuModel[]` |
| `GET /api/Sys/menus/by-parent/{parentId}` | none | `SysMenuModel[]` |
| `GET /api/Sys/menus/by-url?url=/sys/user` | none | `SysMenuModel` or `null` |
| `POST /api/Sys/menus/{id}/freeze` | `{}` | `true` or `false` |
| `POST /api/Sys/menus/{id}/unfreeze` | `{}` | `true` or `false` |

### Roles

| Method and URL | Request Body | Response `data` |
| --- | --- | --- |
| `GET /api/Sys/roles/{roleId}/menus` | none | `SysMenuModel[]` |
| `GET /api/Sys/roles/{roleId}/menus/{menuId}/exists` | none | `true` or `false` |
| `POST /api/Sys/roles/{roleId}/menus` | `{ "menuId": 2 }` | `true` or `false` |
| `DELETE /api/Sys/roles/{roleId}/menus/{menuId}` | none | `true` or `false` |

### Params

| Method and URL | Request Body | Response `data` |
| --- | --- | --- |
| `GET /api/Sys/params/by-key/{key}` | none | `SysParamModel` or `null` |
| `PUT /api/Sys/params/by-key/{key}` | `{ "value": "abc" }` | `true` or `false` |

### Role Auths

| Method and URL | Request Body | Response `data` |
| --- | --- | --- |
| `GET /api/Sys/role-auths/by-role/{roleId}` | none | `SysRoleAuthModel[]` |
| `GET /api/Sys/role-auths/by-menu/{menuId}` | none | `SysRoleAuthModel[]` |
| `GET /api/Sys/role-auths/{roleId}/{menuId}/exists` | none | `true` or `false` |
| `POST /api/Sys/role-auths` | `{ "roleId": 1, "menuId": 2 }` | `true` or `false` for named assign; created entity for standard Add |
| `DELETE /api/Sys/role-auths/{roleId}/{menuId}` | none | `true` or `false` |
| `POST /api/Sys/role-auths/batch` | `{ "roleId": 1, "menuIds": [1, 2, 3] }` | `true` or `false` |
| `DELETE /api/Sys/role-auths/by-role/{roleId}` | none | `true` or `false` |

## Template Named Endpoints

### Products

Product search uses the standard Query endpoint with these filters:

```http
GET /api/Template/products?page=1&pageSize=10&keyword=Key&filters.categoryId=1&filters.status=0&filters.minPrice=10&filters.maxPrice=200&filters.startDate=2026-06-01T00:00:00.0000000Z&filters.endDate=2026-06-23T00:00:00.0000000Z&sortBy=name&desc=false
```

| Method and URL | Request Body | Response `data` |
| --- | --- | --- |
| `GET /api/Template/products/export?...same filters as query...` | none | `ProductModel[]` |
| `GET /api/Template/products/code-exists?code=EL-KEY-001&excludeId=1` | none | `true` or `false` |
| `GET /api/Template/products/count-by-category/{categoryId}` | none | number |
| `POST /api/Template/products/batch-delete` | `{ "ids": [1, 2] }` | affected row count |
| `POST /api/Template/products/batch-status` | `{ "ids": [1, 2], "status": 1 }` | affected row count |
| `POST /api/Template/products/batch-category` | `{ "ids": [1, 2], "categoryId": 3 }` | affected row count |

### Categories

| Method and URL | Request Body | Response `data` |
| --- | --- | --- |
| `GET /api/Template/categories/active` | none | `CategoryModel[]` |
| `GET /api/Template/categories/tree` | none | `CategoryModel[]` |
| `GET /api/Template/categories/children?parentId=1` | none | `CategoryModel[]` |
| `GET /api/Template/categories/name-exists?name=Electronics&excludeId=1` | none | `true` or `false` |

For root categories, omit `parentId`: `GET /api/Template/categories/children`.

### Import Records

| Method and URL | Request Body | Response `data` |
| --- | --- | --- |
| `GET /api/Template/import-records/recent?count=10` | none | `ImportRecordModel[]` |
| `GET /api/Template/import-records/date-range?startDate=2026-06-01T00:00:00.0000000Z&endDate=2026-06-23T00:00:00.0000000Z` | none | `ImportRecordModel[]` |
| `GET /api/Template/import-records?page=1&pageSize=10&sortBy=CreateAt&desc=true` | none | `PagedResult<ImportRecordModel>` |

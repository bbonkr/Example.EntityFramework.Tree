# Example.EntityFramework.Tree

## Model

| Name | Key | Type |  Note |
| :--- | :--: | :---- | :---- |
| Id | PK | string | |
| Name | | string | |
| Url | | string | Nullable |
| Order | | int | |
| Level | | int | |
| ParentId | FK | string | Nullable; Top item does not have parent |

## EntityFramework

### Generate migrations

```shell
$ cd src/Example.EntityFramework.Tree.Data
$ dotnet ef migrations add "<Message: init db>" --startup-project ../Example.EntityFramework.Tree --project ../Example.EntityFramework.Tree.Data.SqlServer --context AppDbContext --json
```



# TP N°3 : Gestion des utilisateurs, authentification et autorisation avec ASP.NET Identity

**Enseignant :** Malek Zribi

---

## Ajouter l'authentification

Dans le but d'intégrer un module d'authentification et de gestion des utilisateurs dans l'application du TP N°2, on va suivre les étapes suivantes :

1. **Installation de l'API Identity** : installer le package `Microsoft.AspNetCore.Identity`.
2. Nous utiliserons Entity Framework Core avec Identity. Par conséquent, nous devons installer le package Identity EF Core : `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.
3. Modifier le code de la classe de contexte en ajoutant un héritage de la classe `IdentityDbContext` :

```csharp
public class AppDbContext : IdentityDbContext
{
    // Reste du code
}
```

4. Configurer les services Identity de ASP.NET Core dans le fichier `Program.cs` en ajoutant ce service :

```csharp
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();
```

5. Ajouter le middleware d'authentification au pipeline de requêtes dans `Program.cs` :

```csharp
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
```

6. Lancer une migration Entity Framework et générer les tables Identity dans la base de données de l'application :

```
Add-Migration MajIdentity
Update-Database
```

Les tables générées dans la base `MyBaseDB` sont : `dbo.__EFMigrationsHistory`, `dbo.AspNetRoleClaims`, `dbo.AspNetRoles`, `dbo.AspNetUserClaims`, `dbo.AspNetUserLogins`, `dbo.AspNetUserRoles`, `dbo.AspNetUsers`, `dbo.AspNetUserTokens`, `dbo.Categories`, `dbo.Products`.

---

## Création de l'interface utilisateur

### RegisterViewModel

Ajouter la classe `RegisterViewModel` suivante dans un dossier `ViewModels` :

```csharp
using System.ComponentModel.DataAnnotations;

namespace TP3_Identity.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password",
            ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
```

### AccountController

Ajouter un contrôleur vide nommé `AccountController` dans lequel vous allez ajouter la méthode d'action suivante :

```csharp
[HttpGet]
public IActionResult Register()
{
    return View();
}
```

### Vue Register.cshtml

Ajouter la vue `Register.cshtml` vide correspondante à cette méthode d'action avec ce code :

```cshtml
@model TP3_Identity.ViewModels.RegisterViewModel
@{
    ViewBag.Title = "User Registration";
}

<h1>User Registration</h1>
<hr />

<div class="row">
    <div class="col-md-4">
        <form method="post">
            <div asp-validation-summary="All" class="text-danger"></div>

            <div class="form-group">
                <label asp-for="Email" class="control-label"></label>
                <input asp-for="Email" class="form-control" />
                <span asp-validation-for="Email" class="text-danger"></span>
            </div>

            <div class="form-group">
                <label asp-for="Password" class="control-label"></label>
                <input asp-for="Password" class="form-control" />
                <span asp-validation-for="Password" class="text-danger"></span>
            </div>

            <div class="form-group">
                <label asp-for="ConfirmPassword" class="control-label"></label>
                <input asp-for="ConfirmPassword" class="form-control" />
                <span asp-validation-for="ConfirmPassword" class="text-danger"></span>
            </div>

            <div class="form-group">
                <input type="submit" value="Register" class="btn btn-primary" />
            </div>
        </form>
    </div>
</div>
```

### Lien Register dans le menu

Ajouter un lien à la vue Register dans le menu de votre application :

```html
<li class="nav-item">
    <a class="nav-link active" asp-controller="Account" asp-action="Register"> Register </a>
</li>
```

---

## Ajout de nouvel utilisateur

Modifions le contrôleur `AccountController` pour pouvoir ajouter des utilisateurs :

```csharp
public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> userManager;
    private readonly SignInManager<IdentityUser> signInManager;

    public AccountController(UserManager<IdentityUser> userManager,
                             SignInManager<IdentityUser> signInManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Copy data from RegisterViewModel to IdentityUser
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            // Store user data in AspNetUsers database table
            var result = await userManager.CreateAsync(user, model.Password);

            // If user is successfully created, sign-in the user using
            // SignInManager and redirect to index action of HomeController
            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("index", "home");
            }

            // If there are any errors, add them to the ModelState object
            // which will be displayed by the validation summary tag helper
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }
}
```

### Configuration des mots de passe

Les paramètres par défaut sont gérés dans une classe `PasswordOptions` avec :

| Propriété | Description | Par défaut |
|---|---|---|
| `RequireDigit` | Nécessite un nombre compris entre 0 et 9 dans le mot de passe. | `true` |
| `RequiredLength` | Longueur minimale du mot de passe. | `6` |
| `RequireLowercase` | Nécessite un caractère minuscule dans le mot de passe. | `true` |
| `RequireNonAlphanumeric` | Nécessite un caractère non alphanumérique dans le mot de passe. | `true` |
| `RequiredUniqueChars` | S'applique uniquement à ASP.NET Core 2.0 ou version ultérieure. Nécessite le nombre de caractères distincts dans le mot de passe. | `1` |
| `RequireUppercase` | Nécessite un caractère majuscule dans le mot de passe. | `true` |

Pour changer la configuration par défaut des mots de passe, appliquer les modifications suivantes :

```csharp
builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
});
```

---

## Afficher et masquer les liens Login et Logout

- Selon que l'utilisateur est connecté ou non, on va voir comment afficher ou masquer les liens **Login**, **Logout** et **Register**.
- Si l'utilisateur n'est pas connecté, on affiche les liens **Login** et **Register** dans la Navbar.
- Une fois l'utilisateur connecté, on affiche le lien **Logout** (suivi de son e-mail) dans la Navbar.

### Fichier `_Layout.cshtml`

```cshtml
@using Microsoft.AspNetCore.Identity
@inject SignInManager<IdentityUser> SignInManager

<div class="collapse navbar-collapse" id="collapsibleNavbar">
    <ul class="navbar-nav ml-auto">
        @*If the user is signed-in display Logout link*@
        @if (SignInManager.IsSignedIn(User))
        {
            <li class="nav-item">
                <form method="post" asp-controller="account" asp-action="logout">
                    <button type="submit" style="width:auto"
                            class="nav-link btn btn-link py-0">
                        Logout @User.Identity.Name
                    </button>
                </form>
            </li>
        }
        else
        {
            <li class="nav-item">
                <a class="nav-link" asp-controller="Account" asp-action="Register">
                    Register
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" asp-controller="Account" asp-action="Login">
                    Login
                </a>
            </li>
        }
    </ul>
</div>
```

### Fichier `AccountController`

```csharp
[HttpPost]
public async Task<IActionResult> Logout()
{
    await signInManager.SignOutAsync();
    return RedirectToAction("Index", "Home");
}
```

---

## Implémentation de la fonctionnalité Login

### LoginViewModel

Ajouter la classe suivante au dossier `ViewModels`. Pour connecter un utilisateur, nous avons besoin de son e-mail (qui est le nom d'utilisateur) et du mot de passe.

```csharp
public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
```

### Actions Login

Dans `AccountController`, ajouter les méthodes d'actions suivantes :

```csharp
[HttpGet]
public IActionResult Login()
{
    return View();
}

[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
{
    if (ModelState.IsValid)
    {
        var result = await signInManager.PasswordSignInAsync(model.Email,
            model.Password, model.RememberMe, false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
    }
    return View(model);
}
```

### Vue Login.cshtml

Ajouter la vue `Login.cshtml` vide correspondante à la méthode d'action `Login` avec ce code :

```cshtml
@model TP3_Identity.ViewModels.LoginViewModel
@{
    ViewBag.Title = "User Login";
}

<h1>User Login</h1>

@{
    var returnUrl = Context.Request.Query["ReturnUrl"];
}

<div class="row">
    <div class="col-md-12">
        <form asp-route-returnurl="@returnUrl" method="post">
            <div asp-validation-summary="All" class="text-danger"></div>

            <div class="form-group">
                <label asp-for="Email"></label>
                <input asp-for="Email" class="form-control" />
                <span asp-validation-for="Email" class="text-danger"></span>
            </div>

            <div class="form-group">
                <label asp-for="Password"></label>
                <input asp-for="Password" class="form-control" />
                <span asp-validation-for="Password" class="text-danger"></span>
            </div>

            <div class="form-group">
                <div class="checkbox">
                    <label asp-for="RememberMe">
                        <input asp-for="RememberMe" />
                        @Html.DisplayNameFor(m => m.RememberMe)
                    </label>
                </div>
            </div>

            <button type="submit" class="btn btn-primary">Login</button>
        </form>
    </div>
</div>
```

---

## Ajouter une autorisation d'accès au contrôleur

Pour rendre nos contrôleurs `ProductController` et `CategoryController` accessibles uniquement après authentification, on va ajouter l'annotation `[Authorize]` :

```csharp
[Authorize]
public class CategoryController : Controller
{
    …
}

[Authorize]
public class ProductController : Controller
{
    …
}
```

Maintenant, l'accès à Product et Category nécessite obligatoirement une authentification. **Testez votre application !**

Pour autoriser l'accès à la méthode d'action `Index` des contrôleurs, on va ajouter l'annotation `[AllowAnonymous]` :

```csharp
// GET: CategoryController
[AllowAnonymous]
public ActionResult Index()
{
    var Categories = CategRepository.GetAll();
    return View(Categories);
}

// GET: ProductController
[AllowAnonymous]
public ActionResult Index()
{
    var Products = ProductRepository.GetAll();
    return View(Products);
}
```

---

## Création de rôles

Pour mieux gérer notre application, nous allons ajouter un administrateur dont le rôle est l'ajout d'utilisateurs et l'attribution des rôles pour chacun. Pour cela, on commence par créer une nouvelle classe `CreateRoleViewModel` dans le dossier `ViewModels` :

```csharp
using System.ComponentModel.DataAnnotations;

namespace TP3_Identity.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; }
    }
}
```

Créer le contrôleur `AdminController` suivant :

```csharp
public class AdminController : Controller
{
    private readonly RoleManager<IdentityRole> roleManager;

    public AdminController(RoleManager<IdentityRole> roleManager)
    {
        this.roleManager = roleManager;
    }

    [HttpGet]
    public IActionResult CreateRole()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
    {
        if (ModelState.IsValid)
        {
            IdentityRole role = new IdentityRole { Name = model.RoleName };
            IdentityResult result = await roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction("ListRoles");
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }

    // Reste du code
}
```

Ajouter la vue correspondante à l'action `CreateRole` :

```cshtml
@model TP3_Identity.ViewModels.CreateRoleViewModel
@{
    ViewBag.Title = "Create New Role";
}

<h1>Create New Role</h1>

<form asp-action="CreateRole" method="post" class="mt-3">
    <div asp-validation-summary="All" class="text-danger"></div>

    <div class="form-group row">
        <label asp-for="RoleName" class="col-sm-2 col-form-label"></label>
        <div class="col-sm-10">
            <input asp-for="RoleName" class="form-control" placeholder="Name">
            <span asp-validation-for="RoleName" class="text-danger"></span>
        </div>
    </div>

    <div class="form-group row">
        <div class="col-sm-10">
            <button type="submit" class="btn btn-primary" style="width:auto">
                Create Role
            </button>
        </div>
    </div>
</form>
```

---

## Affichage liste des rôles

Pour gérer tous les rôles de l'application, on va ajouter la méthode d'action `ListRoles` suivante :

```csharp
[HttpGet]
public IActionResult ListRoles()
{
    var roles = roleManager.Roles;
    return View(roles);
}
```

Et la vue correspondante à cette méthode d'action :

```cshtml
@using Microsoft.AspNetCore.Identity
@model IEnumerable<IdentityRole>
@{
    ViewBag.Title = "All Roles";
}

<h1>All Roles</h1>

@if (Model.Any())
{
    <a class="btn btn-primary mb-3" style="width:auto" asp-action="CreateRole"
       asp-controller="Admin">Add new role</a>

    foreach (var role in Model)
    {
        <div class="card mb-3">
            <div class="card-header"> Role Id : @role.Id </div>
            <div class="card-body"> <h5 class="card-title">@role.Name</h5> </div>
            <div class="card-footer">
                <a href="#" class="btn btn-primary">Edit</a>
                <a href="#" class="btn btn-danger">Delete</a>
            </div>
        </div>
    }
}
else
{
    <div class="card">
        <div class="card-header">
            No roles created yet
        </div>
        <div class="card-body">
            <h5 class="card-title">
                Use the button below to create a role
            </h5>
            <a class="btn btn-primary" style="width:auto"
               asp-controller="Admin" asp-action="CreateRole">
                Create Role
            </a>
        </div>
    </div>
}
```

---

## Modification d'un rôle

Pour pouvoir modifier les rôles déjà créés, on a besoin d'ajouter la classe `EditRoleViewModel` suivante dans le dossier `ViewModels` :

```csharp
public class EditRoleViewModel
{
    public EditRoleViewModel()
    {
        Users = new List<string>();
    }

    public string Id { get; set; }

    [Required(ErrorMessage = "Role Name is required")]
    public string RoleName { get; set; }

    public List<string> Users { get; set; }
}
```

Au niveau du contrôleur `AdminController`, nous allons ajouter cette déclaration et modifier le constructeur :

```csharp
public class AdminController : Controller
{
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly UserManager<IdentityUser> userManager;

    public AdminController(RoleManager<IdentityRole> roleManager,
                           UserManager<IdentityUser> userManager)
    {
        this.roleManager = roleManager;
        this.userManager = userManager;
    }
```

Et nous allons aussi ajouter cette méthode d'action (GET) :

```csharp
// Role ID is passed from the URL to the action
[HttpGet]
public async Task<IActionResult> EditRole(string id)
{
    // Find the role by Role ID
    var role = await roleManager.FindByIdAsync(id);

    if (role == null)
    {
        ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
        return View("NotFound");
    }

    var model = new EditRoleViewModel
    {
        Id = role.Id,
        RoleName = role.Name
    };

    // Retrieve all the Users
    foreach (var user in userManager.Users.ToList())
    {
        // If the user is in this role, add the username to
        // Users property of EditRoleViewModel. This model
        // object is then passed to the view for display
        if (await userManager.IsInRoleAsync(user, role.Name))
        {
            model.Users.Add(user.UserName);
        }
    }

    return View(model);
}
```

Ainsi que cette méthode d'action (POST) :

```csharp
// This action responds to HttpPost and receives EditRoleViewModel
[HttpPost]
public async Task<IActionResult> EditRole(EditRoleViewModel model)
{
    var role = await roleManager.FindByIdAsync(model.Id);

    if (role == null)
    {
        ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
        return View("NotFound");
    }
    else
    {
        role.Name = model.RoleName;

        // Update the Role using UpdateAsync
        var result = await roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            return RedirectToAction("ListRoles");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }
}
```

---

## Suppression d'un rôle

Pour pouvoir supprimer un rôle, nous allons commencer par ajouter cette méthode d'action :

```csharp
[HttpPost]
public async Task<IActionResult> DeleteRole(string id)
{
    var role = await roleManager.FindByIdAsync(id);

    if (role == null)
    {
        ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
        return View("NotFound");
    }
    else
    {
        var result = await roleManager.DeleteAsync(role);

        if (result.Succeeded)
        {
            return RedirectToAction("ListRoles");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View("ListRoles");
    }
}
```

---

## Modification / Suppression d'un rôle (interface)

Au niveau de la vue `ListRoles`, modifier le code des boutons **Edit** et **Delete** comme suit :

```cshtml
<div class="card-footer">
    <form method="post" asp-action="DeleteRole" asp-route-id="@role.Id">
        <a asp-controller="Admin" asp-action="EditRole"
           asp-route-id="@role.Id" class="btn btn-primary">Edit</a>

        <span id="confirmDeleteSpan_@role.Id" style="display:none">
            <span>Are you sure you want to delete?</span>
            <button type="submit" class="btn btn-danger">Yes</button>
            <a href="#" class="btn btn-primary"
               onclick="confirmDelete('@role.Id', false)">No</a>
        </span>

        <span id="deleteSpan_@role.Id">
            <a href="#" class="btn btn-danger"
               onclick="confirmDelete('@role.Id', true)">Delete</a>
        </span>
    </form>
</div>
```

Dans le sous-dossier `js` du dossier `wwwroot`, ajouter un fichier nommé `CustomScript.js` contenant le code suivant :

```javascript
function confirmDelete(uniqueId, isDeleteClicked) {
    var deleteSpan = 'deleteSpan_' + uniqueId;
    var confirmDeleteSpan = 'confirmDeleteSpan_' + uniqueId;

    if (isDeleteClicked) {
        $('#' + deleteSpan).hide();
        $('#' + confirmDeleteSpan).show();
    } else {
        $('#' + deleteSpan).show();
        $('#' + confirmDeleteSpan).hide();
    }
}
```

Au niveau du fichier `_Layout.cshtml`, ajouter cette instruction pour faire appel à la fonction précédemment créée :

```html
<script src="~/js/CustomScript.js"></script>
```

Ajouter le bouton **Administration** dans la NavBar qui permet d'afficher la vue `ListRoles`.

Exécutez votre application et ajoutez les rôles **Admin**, **User** et **Manager**.

---

## Ajout et suppression d'utilisateurs d'un rôle donné

- Dans cette partie, nous allons programmer les actions **Add Users** et **Remove Users** pour un rôle donné.
- Pour ajouter ou supprimer un utilisateur, il suffit de cocher ou décocher la case correspondante dans la liste des utilisateurs.

On va alors ajouter la classe `UserRoleViewModel` dans le dossier `ViewModels` :

```csharp
public class UserRoleViewModel
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public bool IsSelected { get; set; }
}
```

### Vue EditRole

Commençons par ajouter la vue `EditRole` suivante :

```cshtml
@model TP3_Identity.ViewModels.EditRoleViewModel
@{
    ViewBag.Title = "Edit Role";
}

<h1>Edit Role</h1>

<form method="post" class="mt-3">
    <div class="form-group row">
        <label asp-for="Id" class="col-sm-2 col-form-label"></label>
        <div class="col-sm-10">
            <input asp-for="Id" disabled class="form-control">
        </div>
    </div>

    <div class="form-group row">
        <label asp-for="RoleName" class="col-sm-2 col-form-label"></label>
        <div class="col-sm-10">
            <input asp-for="RoleName" class="form-control">
            <span asp-validation-for="RoleName" class="text-danger"></span>
        </div>
    </div>

    <div asp-validation-summary="All" class="text-danger"></div>

    <div class="form-group row">
        <div class="col-sm-10">
            <button type="submit" class="btn btn-primary">Update</button>
            <a asp-action="ListRoles" class="btn btn-primary">Cancel</a>
        </div>
    </div>

    <div class="card">
        <div class="card-header">
            <h3>Users in this role</h3>
        </div>
        <div class="card-body">
            @if (Model.Users.Any())
            {
                foreach (var user in Model.Users)
                {
                    <h5 class="card-title">@user</h5>
                }
            }
            else
            {
                <h5 class="card-title">None at the moment</h5>
            }
        </div>
        <div class="card-footer">
            <a asp-controller="Admin" asp-action="EditUsersInRole"
               asp-route-roleId="@Model.Id" class="btn btn-primary">
                Add or Remove Users from this Role
            </a>
        </div>
    </div>
</form>
```

### Actions EditUsersInRole

Au niveau du contrôleur `AdminController`, nous allons ajouter les méthodes d'action suivantes :

```csharp
[HttpGet]
public async Task<IActionResult> EditUsersInRole(string roleId)
{
    ViewBag.roleId = roleId;

    var role = await roleManager.FindByIdAsync(roleId);

    if (role == null)
    {
        ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
        return View("NotFound");
    }

    var model = new List<UserRoleViewModel>();

    foreach (var user in userManager.Users.ToList())
    {
        var userRoleViewModel = new UserRoleViewModel
        {
            UserId = user.Id,
            UserName = user.UserName
        };

        if (await userManager.IsInRoleAsync(user, role.Name))
        {
            userRoleViewModel.IsSelected = true;
        }
        else
        {
            userRoleViewModel.IsSelected = false;
        }

        model.Add(userRoleViewModel);
    }
    return View(model);
}
```

```csharp
[HttpPost]
public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
{
    var role = await roleManager.FindByIdAsync(roleId);

    if (role == null)
    {
        ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
        return View("NotFound");
    }

    for (int i = 0; i < model.Count; i++)
    {
        var user = await userManager.FindByIdAsync(model[i].UserId);

        IdentityResult result = null;

        if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
        {
            result = await userManager.AddToRoleAsync(user, role.Name);
        }
        else if (!model[i].IsSelected && await userManager.IsInRoleAsync(user, role.Name))
        {
            result = await userManager.RemoveFromRoleAsync(user, role.Name);
        }
        else
        {
            continue;
        }

        if (result.Succeeded)
        {
            if (i < (model.Count - 1))
                continue;
            else
                return RedirectToAction("EditRole", new { Id = roleId });
        }
    }

    return RedirectToAction("EditRole", new { Id = roleId });
}
```

### Vue EditUsersInRole

Le code de la vue `EditUsersInRole` sera le suivant :

```cshtml
@using TP3_Identity.ViewModels
@model List<UserRoleViewModel>
@{
    var roleId = ViewBag.roleId;
}

<form method="post">
    <div class="card">
        <div class="card-header">
            <h2>Add or remove users from this role</h2>
        </div>
        <div class="card-body">
            @for (int i = 0; i < Model.Count; i++)
            {
                <div class="form-check m-1">
                    <input type="hidden" asp-for="@Model[i].UserId" />
                    <input type="hidden" asp-for="@Model[i].UserName" />
                    <input asp-for="@Model[i].IsSelected" class="form-check-input" />
                    <label class="form-check-label" asp-for="@Model[i].IsSelected">
                        @Model[i].UserName
                    </label>
                </div>
            }
        </div>
        <div class="card-footer">
            <input type="submit" value="Update" class="btn btn-primary"
                   style="width:auto" />
            <a asp-action="EditRole" asp-route-id="@roleId"
               class="btn btn-primary" style="width:auto">Cancel</a>
        </div>
    </div>
</form>
```

Ensuite :

- Exécutez votre application.
- Créez des utilisateurs et précisez le rôle de chacun.

---

## Attribution d'autorisation pour un rôle donné

On va interdire l'accès au contrôleur `Admin` pour les non-membres du rôle **Admin** en ajoutant cette instruction dans le code de `AdminController` :

```csharp
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    ...
}
```

Au niveau du contrôleur `AccountController`, ajouter cette méthode d'action :

```csharp
[AllowAnonymous]
public IActionResult AccessDenied()
{
    return View();
}
```

Et créer la vue `AccessDenied` correspondante avec le code suivant :

```cshtml
<div class="text-center">
    <h1 class="text-danger">Access Denied</h1>
    <h6 class="text-danger">You do not have persmission to view this resource</h6>
    <img src="~/images/noaccess.png" style="height:300px; width:300px" />
</div>
```

> **Remarque :** pour l'image, ajouter un dossier `images` sous le dossier root de votre application et y ajouter une image nommée `noaccess.png`.

Un utilisateur dont le rôle est différent de **Admin** n'aura plus l'autorisation d'accéder à l'administration et la page « Access Denied » sera affichée.

---

## Afficher ou masquer le menu de navigation en fonction du rôle de l'utilisateur connecté

Le menu de navigation **Administration** ne sera affiché que si l'utilisateur connecté dispose du rôle **Admin**. Pour cela, au niveau du fichier `_Layout.cshtml`, ajouter le code suivant :

```cshtml
@if (SignInManager.IsSignedIn(User) && User.IsInRole("Admin"))
{
    <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle" data-bs-toggle="dropdown" href="#"
           role="button" aria-haspopup="true" aria-expanded="false">Administration</a>
        <div class="dropdown-menu">
            <a class="dropdown-item" asp-area="" asp-controller="Admin"
               asp-action="CreateRole">Create Role</a>
            <div class="dropdown-divider"></div>
            <a class="dropdown-item" asp-area="" asp-controller="Admin"
               asp-action="ListRoles">List Roles</a>
        </div>
    </li>
}
```

On aura alors l'affichage suivant :

- **Menu pour un utilisateur dont le rôle est User** : pas de menu Administration.
- **Menu pour un utilisateur dont le rôle est Admin** : menu Administration avec les entrées *Create Role* et *List Roles*.

---

## Ajout de Manager pour Product et Category

On vous demande d'ajouter un nouveau rôle **Manager**. Ce rôle aura la possibilité, avec l'**Admin**, de créer, modifier, supprimer et afficher la liste des Products ou Categories. Le rôle **User** n'aura que la possibilité de consulter les listes.

Pour cela, au niveau des contrôleurs `ProductController` et `CategoryController`, ajouter l'autorisation suivante :

```csharp
[Authorize(Roles = "Admin,Manager")]
public class ProductController : Controller
{
    readonly IProductRepository ProductRepository;
    readonly ICategorieRepository CategRepository;
    private readonly IWebHostEnvironment hostingEnvironment;
    ...

    // GET: ProductController
    [AllowAnonymous]
    public ActionResult Index()
    {
        var Products = ProductRepository.GetAll();
        return View(Products);
    }
}
```

```csharp
[Authorize(Roles = "Admin,Manager")]
public class CategoryController : Controller
{
    readonly ICategorieRepository CategRepository;

    public CategoryController(ICategorieRepository categRepository)
    ...

    // GET: CategoryController
    [AllowAnonymous]
    public ActionResult Index()
    {
        var Categories = CategRepository.GetAll();
        return View(Categories);
    }
}
```

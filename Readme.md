# Giny.NETCore

Emulateur Dofus 2.x (client ~2.68) écrit en C# / .NET 6, dérivé du projet Giny.

## Contenu

| Projet | Rôle |
|---|---|
| `Sources/Servers/Giny.Auth` | Serveur d'authentification (login, sélection de serveur) |
| `Sources/Servers/Giny.World` | Serveur de jeu (cartes, combats, métiers, échanges, quêtes...) |
| `Sources/Giny.Core` | Utilitaires communs (logs, config, pool, extensions) |
| `Sources/Giny.ORM` | ORM maison (records `[Table]`, sauvegarde cyclique) |
| `Sources/Giny.Protocol` | Messages et types du protocole Dofus |
| `Sources/Giny.IO` | Lecteurs de données client (D2O, D2P, DLM...) |
| `Sources/Modules/*` | Modules serveur : patcher de base (interactifs...), drops, zaap donjon, Pokéfus, monstres de forgemagie |
| `Sources/Tools/Giny.WorldEditor` | Éditeur web du monde (items, sorts, monstres, quêtes) |
| `Sources/Tools/Giny.MapEditor` | Éditeur de cartes |
| `Sources/Sync/*` | Outils de synchronisation (D2O → base, builder de protocole, AS3) |
| `Sources/Zaap/*` | Launcher (Uplauncher) et passerelle Zaap |

## Prérequis

- **.NET 6 SDK** (Windows recommandé : certaines étapes post-build utilisent `copy`)
- Une base **SQL** (le nom, hôte et identifiants se configurent dans les fichiers de config des serveurs)
- Un **client Dofus** compatible, dont les données `data/common` (D2O) sont lues par `Giny.IO`

## Build

```bash
dotnet restore Sources/Giny.sln
dotnet build Sources/Servers/Giny.Auth/Giny.Auth.csproj
dotnet build Sources/Servers/Giny.World/Giny.World.csproj
```

> ⚠️ La solution entière en une seule commande échoue parfois : plusieurs projets
> partagent des étapes post-build `copy` sur les mêmes sorties. Compiler chaque
> serveur séparément.

## Lancement

1. Configurer la connexion SQL dans `Giny.Auth` / `Giny.World` (`config.json`)
2. Lancer `Giny.DatabasePatcher` pour préparer/patcher la base (interactifs de métiers, tables manquantes)
3. Démarrer `Giny.Auth` puis `Giny.World`
4. Connecter le client sur le port de l'auth server

## Architecture (world server)

- **`Records/`** — tables persistantes ([Table], [Primary], [Update], [Container])
- **`Managers/`** — logique métier : `Fights` (combats, effets de sorts, zones, buffs, triggers), `Items` (inventaire, effets d'objets), `Maps` (instances, éléments interactifs, NPC, groupes de monstres), `Exchanges` (échanges, métiers, HDV, forgemagie, multicraft), `Dialogs`, `Quests`, `Guilds`...
- **`Handlers/`** — dispatch des messages réseau (`[MessageHandler]`)
- **`Network/`** — client/serveur TCP

### Effets de sorts

`Managers/Fights/Effects/SpellEffectHandler` — un handler par `EffectsEnum`. Les zones
d'effet (`Zones/`) mappent les caractères de zone Dofus (`X`, `C`, `L`, `T`, `B`, `A`...)
vers des formes (`Cross`, `Lozenge`, `Line`, `TShape`...).

### Montures / enclos

`Records/Mounts/MountRecord` + `MountsHandler`. Les certificats de monture créent la
monture à l'utilisation ; `GenericActionEnum.Paddock` (interactif enclos, `!paddock`)
permet de déposer/reprendre sa monture ; `!enclos` stobule depuis n'importe quelle carte.

## API web

Chaque serveur expose une API ASP.NET (`Web/`):

- `POST /account/auth` — connexion (auth server)
- `POST /account/register` — inscription
- `GET /profile/{name}` — profil public d'un personnage
- `GET /news` — actualités (fichier `news.json` éditable)
- `GET /common/stats` — statistiques du serveur en ligne

## Chat commands

Préfixe `!` en jeu. Exemples : `!help`, `!infos`, `!elements`, `!smith`, `!paddock`,
`!enclos`, `!monsters`, `!craft`... (voir `Managers/Chat/ChatCommands.cs`).

## CI

`.github/workflows/build.yml` — build `Sources/Giny.sln` (Windows, .NET 6) sur push/PR.

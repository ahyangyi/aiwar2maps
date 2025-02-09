Tessellation (Map Generator)
============================

# Introduction
This is a tessellation-based map generator. Pick a tessellation type, and use the many other options to transform it in many ways, and enjoy the unique challenge it brings.

# Options
## Tessellation Type
You can choose from a long list of different tessellation types as the basis of the map. It controls the basic texture of the map.

| <img src="Tessellation%20Types/1.jpg?raw=true" alt="Squares" height=200> | <img src="Tessellation%20Types/2.jpg?raw=true" alt="Hexagons" height=200> |<img src="Tessellation%20Types/101.jpg?raw=true" alt="Square Y" height=200> |
|:--:|:--:|:--:|
| Squares | Hexagons | Square Y |

## Symmetry
In this mod, the term “symmetry” is used in a broad sense, and covers many different ways to make parts of the galaxy look like other parts of it.

This option interacts with about all other options in this map generator: the symmetry is always perserved. The only exception is the Beltway, which is *not part of* the symmetry (but does not break it either).

| <img src="Symmetries/100.jpg?raw=true" alt="Asymmetrical" height=200> | <img src="Symmetries/150.jpg?raw=true" alt="Bilateral" height=200> |<img src="Symmetries/200.jpg?raw=true" alt="Rotational" height=200> |<img src="Symmetries/250.jpg?raw=true" alt="Dihedral" height=200> |
|:--:|:--:|:--:|:--:|
| Asymmetrical | Bilateral | Rotational | Dihedral |

| <img src="Symmetries/10000.jpg?raw=true" alt="Juxtaposition" height=200> |
|:--:|
| Juxtaposition |

## Galaxy Shape
This option offers a way to control the general shape of the galaxy. Due to complex interactions with tessellation types and symmetries, we do not offer concrete shapes. Instead, the three options are general ideas that with clear gameplay consequences.

`Default` typically attempts to fill in a rectangular area, unless the symmetry enforces a different one. `Rounded` usually cuts corners and makes it easier to reach from the farthest pair of planets. `Pointy` does the opposite, creates more chances of long paths and potential chokepoints.

| <img src="Symmetries/150.jpg?raw=true" alt="Default" height=200> |<img src="Galaxy%20Shapes/2.jpg?raw=true" alt="Rounded" height=200> |<img src="Galaxy%20Shapes/3.jpg?raw=true" alt="Pointy" height=200> |
|:--:|:--:|:--:|
| Default | Rounded | Pointy |

## Outer Path
Are you a fan of Dismiss's Generator mod and the "outer path" option? Are you a fan of RadiantMaps and their "beltway" option? I am, and hence I offer my interpretation of these options.

Enabling Outer Path ensures the outermost planets and their connections are always preserved during galaxy generation. The Beltway option, on the other hand, draws an extra ring of planets to provide a way to quickly move between different parts of the galaxy.

## Dissonance
The heart of many good map generators, dissonance simply decides how many planets to remove during the map generation. I compensate this by generating more planets than asked for, then removing some of them.

## Connectivity
Controls the number of links between the planets. The lowest option only places as many links as needed to keep the galaxy connected, while the highest option tries to add as many links as allowed by other options.

# Inspiration
This map generator is inspired by many existing map types, and in particular, the ones below:

* **Grid**: the basis of everything else, and also the "crosshatch" option in particular.
* **GridEx** (from Dismiss's Generator): Aspect ratios, outline mode.
* **Honeycomb**: solar snake mode.
* **Maze**: minimally connected system, winding paths.
* **Mirrored**(from Dismiss's Generator) : symmetry.
* **Radiant's Map Generators**: beltway option, wibble slider.

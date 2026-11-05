# Mixly engine third-party notices

The School Bot adapter is separate from upstream Mixly. Mixly's name, authorship,
source headers and license are retained. `source-manifest.json` records the exact
imported files using SHA256 over UTF-8 text with LF endings. The original package
identifies its author as Mixly and version as `2.0rc3`.

| Component | Pinned version/source | License text shipped in `licenses/` |
| --- | --- | --- |
| Mixly generators, board profiles and configuration | `sb-app-mixly/resources/app/src`; per-file hashes in `source-manifest.json` | `MIXLY-LICENSE.txt` (original retention terms) |
| Blockly bundled by Mixly; Google Inc. and contributors | The imported Mixly snapshot; original copyright/author headers retained | `BLOCKLY-LICENSE.txt` (Apache-2.0) |
| Go standard library | Go toolchain used by the build; minimum Go 1.26 | `GO-LICENSE.txt` (BSD-3-Clause) |
| github.com/dop251/goja | v0.0.0-20261020172427-7b1a02208696 | `GOJA-LICENSE.txt` (MIT) |
| github.com/dlclark/regexp2/v2 | v2.8.1 | `REGEXP2-LICENSE.txt` (MIT) |
| github.com/go-sourcemap/sourcemap | v2.1.3+incompatible | `SOURCEMAP-LICENSE.txt` (BSD-2-Clause) |
| github.com/google/pprof | v0.0.0-20230207041349-798e818bf904 | `PPROF-LICENSE.txt` (Apache-2.0) |
| go.bug.st/serial | v1.6.4 | `SERIAL-LICENSE.txt` (BSD-3-Clause) |
| github.com/creack/goselect | v0.1.2 | `GOSELECT-LICENSE.txt` (MIT) |
| golang.org/x/sys | v0.19.0 | `X-SYS-LICENSE.txt` (BSD-3-Clause) |
| golang.org/x/text | v0.43.0 | `X-TEXT-LICENSE.txt` (BSD-3-Clause) |

Upstream repository: https://gitee.com/mixly2/mixly2.0_src

The upstream AVR serial sample in `tests/fixtures/avr-serial.mix` is test input
from `sample/Arduino AVR/04-文本/01-serial_string-1.mix`; it is not in the binary.

## Bundled Arduino libraries (`libraries/`)

Shipped as source beside the engine because arduino-cli compiles them into each Block Lab
program. Each folder is the upstream library without examples, documentation, images and
editor files; the remaining sources and license files are unmodified. Their license texts
stay inside each library folder.

| Library (folder) | Version | Copied from | License (file) |
| --- | --- | --- | --- |
| Servo (`Servo`) | 1.2.1 | sb-app-mixly `arduino-cli/libraries/Servo` | LGPL-2.1 (`LICENSE.txt`) |
| Adafruit NeoPixel (`Adafruit_NeoPixel`) | 1.12.0 | sb-app-mixly `arduino-cli/libraries/Adafruit_NeoPixel` | LGPL-3.0 (`COPYING`) |
| U8g2 (`U8g2`) | 2.34.22 | sb-app-mixly `arduino-cli/libraries/U8g2` | BSD-2-Clause; fonts under the licenses listed in `LICENSE` |
| IRremote (`IRremote`) | 3.7.1 | sb-app-mixly `arduino-cli/libraries/IRremote` | MIT (`LICENSE`; contributors in `Contributors.md`) |
| ZLTech dht (`ZLTech_dht`) | 0.1.01: Rob Tillaart's DHTLib, modified by ZLTech | Mixly Thai build, ZLTech UNO package V2.0.9, `libraries/ZLTech_dht` | Source purchased from ZLTech by the School Bot publisher. Based on Rob Tillaart's DHTLib 0.1.01 (Arduino Playground); the files carry no license text. |
| ZLTech APDS9960 (`ZLTech_APDS9960`) | 1.0.0 (RTrobot) | Same package, `libraries/ZLTech_APDS9960` | Beerware, stated in `src/APDS9960.h` |
| ZLTech Adafruit TCS34725 (`ZLTech_Adafruit_TCS34725`) | 1.3.1 (Adafruit) with ZLTech's `getR/getG/getB/getC` | Same package, `libraries/ZLTech_Adafruit_TCS34725` | BSD-3-Clause (`license.txt`) |

The LGPL libraries are distributed as source, so recipients can change and rebuild them. A
program compiled with them is built on the user's computer; School Bot does not distribute it.

The engine does not redistribute Electron, Arduino CLI, Python, mpremote, esptool,
kflash, board cores or firmware images. A toolchain distribution
must preserve those components' own notices and licenses when it supplies them.
The engine's binary embeds the imported generator text. This is not encryption
and does not prevent inspection or reverse engineering.

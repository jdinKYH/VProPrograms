# VProSTD

VisionPro 검사 프로그램. **이미지 취득과 검사(ToolBlock)를 분리**한 구조다.
VProEditAll 의 안전 규칙(바이트 확인 후 경고, 안전한 저장, 종료 시 카메라 해제)을 이어받았다.
작업 기록과 시험 결과는 `WORKLOG.md` 참고.

```
[이미지 입력]                                  [검사]                                  [결과]
 File Mode 켜짐 → 이미지 파일 / 폴더   ─┐
                                        ├→ ToolBlock.Inputs["InputImage"] → Run() → 판정(CogResultsAnalysisTool) → OK/NG, 카운트, Log, CSV, 이미지 저장
 File Mode 꺼짐 → 카메라 취득 (코드로 연결) ─┘
```

---

## 1. 운영 규칙 (사용하는 사람)

1. **출처를 모르는 vpp 파일은 레시피에 넣지 않는다.** vpp 는 프로그램 코드(스크립트)를 포함할 수 있다.
2. **장비 PC 에서는 QuickBuild / 다른 생산 프로그램을 먼저 종료한다.** 같은 카메라를 두 프로그램이 동시에 쓰면 연결이 끊어진다.
3. **카메라는 이 프로그램을 종료해야 해제된다.**
4. 레시피 SAVE 를 하면 이전 파일이 `.bak` 으로 1개 남는다 (저장할 때마다 덮어씀).
5. AUTO RUN 중에는 탭 이동·레시피 변경·종료가 막힌다. STOP 을 먼저 누른다.

## 2. 레시피 (ToolBlock .vpp 약속)

레시피 = Recipe Path 안의 ToolBlock `.vpp` 파일 하나. 파일 이름(확장자 제외)이 레시피 이름이다.

```
<Recipe Path>/<레시피 이름>.vpp          검사 ToolBlock                    ← 필수
<Recipe Path>/<레시피 이름>.recipe.xml   레시피 설정 (노출 포함, 없으면 기본값)
```

- 카메라 설정(CogAcqFifoTool) `.vpp` 는 쓰지 않는다. Recipe Path 에 있어도 목록에 나오지 않는다.

### 2-1. Job(ToolBlock) 작성 규칙

레시피를 불러올 때 필수 항목(1, 2)이 없으면 경고 창이 뜬다. 이름을 자동으로 맞춰 주지 않으므로 규칙대로 만든다.

**필수**

1. **이미지 입력**: Inputs 에 이름이 정확히 `InputImage` 인 입력 (형식 `ICogImage` 또는 `CogImage8Grey`, 대소문자까지 같게).
   QuickBuild 에서 가져와 `OutputImage` 등 다른 이름이면 바꾼다. 흑백 입력에 컬러 이미지가 오면 자동으로 흑백 변환한다.
2. **판정**: 최상위 ToolBlock 바로 안에 CogResultsAnalysisTool. 판정(Decision) Accept = OK, Error = ERROR, 나머지 = NG.
   - 판정 툴이 여러 개이면 `<이름>.recipe.xml` 의 `ResultsAnalysisToolName` 에 쓸 툴 이름을 적는다.
   - 판정 툴이 없으면 Outputs `Result` (bool, true = OK) 로 판정한다 (예전 방식, NEW 로 만든 기본 레시피).
   - 판정 툴 앞의 툴이 Reject/Error 이면 판정 툴이 Accept 여도 NG 로 하고, 결과 표 값은 비운다.
3. **카메라·I/O 를 넣지 않는다**: AcqFifo·I/O 설정이 들어 있으면 불러오기 전에 경고한다. 카메라는 CONNECT CAMERA 탭에서 연결한다.

**선택**

4. Inputs `PixelX`, `PixelY` (double): 있으면 SYSTEM 탭 값을 넣어 준다.
5. **RUN 화면 이미지**: `Locate` ToolBlock 안 `CogFixtureTool1` 의 출력 이미지 (Record `Locate.CogFixtureTool1.OutputImage`).
   없으면 입력 이미지를 보여 준다.
6. **RUN 결과 표**: 판정 툴의 Input / Output 항목이 열이 된다 (항목 이름 = 열 제목).

**스크립트 작성 요령** (자세한 시험 내용은 `WORKLOG.md` 2026-11-19)

7. 최상위 바로 안의 툴은 실패할 때 **Error 가 아니라 Reject** 를 낸다.
   Error 가 나면 최상위 ToolBlock 이 뒤의 툴을 실행하지 않아 이전 검사 값이 남는다 (Reject 는 뒤의 툴을 계속 실행한다).
8. 안쪽 ToolBlock 스크립트는 시작할 때 출력을 0 으로 두고, 툴이 실패하거나 입력 이미지가 없으면 그 자리에서 Reject 로 멈추고,
   성공했을 때만 출력에 값을 넣는다.
9. 실패할 수 있는 툴에서 출력 바인딩(링크)을 쓰지 않는다. 바인딩이 실패하면 결과가 Error 로 바뀐다. 출력은 스크립트에서 직접 넣는다.

**저장**

10. VProSTD 의 SAVE 는 Minimum (툴의 마지막 입력·출력 이미지와 결과 제외, 학습 패턴은 남음) 으로 저장한다.
    QuickBuild 에서 저장할 때도 이미지·결과를 빼고 저장하는 것을 권장한다.

### 2-2. RECIPE 탭

- RECIPE 탭의 목록과 모든 버튼은 SYSTEM 탭의 **Recipe Path** 를 쓴다.
- RECIPE 탭의 NEW 는 Inputs `InputImage` 와 Outputs `Result` 가 들어 있는 빈 ToolBlock 을 만든다.
- RECIPE 탭의 SAVE 는 Recipe Path 에서 파일 저장 창을 연다. 현재 레시피 파일 = 덮어쓰기, 다른 이름 = 다른 이름으로 저장 후 그 레시피로 바뀜. Recipe Path 밖에는 저장하지 않는다.
- RECIPE 탭의 DELETE 는 `<이름>.vpp`, `.vpp.bak`, `<이름>.recipe.xml` 만 지운다.

## 3. 카메라 (CONNECT CAMERA 탭)

카메라는 레시피와 따로 관리한다. `.vpp` 를 불러오지 않고 코드에서 직접 연결한다 (`Core/CameraSources.cs`).

| 버튼 | 하는 일 |
|---|---|
| SEARCH | 연결된 카메라를 찾아 목록에 넣는다 (저장된 카메라를 골라 둠) |
| CONNECT | 목록에서 고른 카메라 → 없으면 저장된 카메라 → 없으면 첫 번째 카메라. 연결되면 현재 레시피 노출을 적용 |
| TRIGGER | 1장 취득해서 보여 줌 (검사 안 함). 레시피 입력에도 넣어 RECIPE 탭에서 바로 실행해 볼 수 있음 |
| LIVE | 한 번 누르면 ON, 다시 누르면 OFF. 계속 취득해서 보여 줌 (ON 중에는 TRIGGER 막음, 다른 탭으로 가면 OFF) |
| 노출 | 바로 카메라에 적용. **레시피별 값**이므로 SAVE CAMERA 또는 RECIPE 탭 SAVE 때 `<이름>.recipe.xml` 에 저장된다. 연결 전에는 레시피 값(없으면 빈칸)을 보여 줌 |
| SAVE CAMERA | 연결한 카메라(시리얼, 영상 형식)를 `system.xml` 의 `Camera` 에, 노출을 현재 레시피 `<이름>.recipe.xml` 에 저장 |

- 프로그램을 시작하면 마지막 레시피를 불러온 뒤 **저장된 카메라(SAVE CAMERA)에 확인 창 없이 자동 연결**한다 (`MainForm_Shown` → `VisionApp.ConnectSavedCamera`).
  저장된 카메라가 없으면 연결하지 않고, 실패하면 경고 창을 띄운다. 끄려면 `system.xml` 의 `Camera.AutoConnect` 를 `false` 로.
  카메라 선택·연결 설정은 엔지니어가 CONNECT CAMERA 탭에서 한다.
- 프로그램을 종료하면 카메라를 해제하고 로그에 `CAMERA: 해제 완료 (n 대)` 를 남긴다.
- 레시피를 바꿔도 카메라 연결은 유지되고, 새 레시피의 노출이 적용된다 (레시피 노출이 0 이면 카메라 값 유지).
- 지금은 VisionPro 가 찾는 카메라(GigE)만 지원한다. Euresys CameraLink 에어리어 카메라는 `ICameraSource` 를 구현해서 `VisionApp.OpenCamera` 에 추가한다.

## 4. 설정과 기록 파일

| 파일 | 위치 | 내용 |
|---|---|---|
| `system.xml` | 실행 파일 옆 (고정) | SYSTEM 탭 값, 마지막 레시피 (다음 실행 때 자동으로 불러옴), 카메라 설정 (`Camera`, 시작 때 자동 연결 `AutoConnect`) |
| 설정 파일 사본 (`*.xml`) | System Path (SAVE 창에서 고름) | SYSTEM 탭 SAVE 는 고른 파일과 위 system.xml 에 함께 저장, LOAD 는 고른 파일을 화면에 읽어 옴 (SAVE 해야 적용) |
| `yyyyMMdd_log.txt` | Log Path | 화면 Log 와 같은 내용 |
| `yyyyMMdd_result.csv` | Log Path | 검사 1회 = 1줄 (Time, Source, Judge, TimeMs, 결과 표 값) |
| `HHmmss_fff_Origin/Result.bmp(jpg)` | Result Path/yyyyMMdd/OK·NG | 원본 / 검사 그래픽이 그려진 화면 (SYSTEM 탭에서 선택) |

경로를 비워 두면 실행 파일 옆 `Data\` 아래(System, Recipe, Log, Result)로 정해진다.

## 5. 코드 읽는 순서

| 순서 | 파일 | 하는 일 |
|---|---|---|
| 1 | `Program.cs` | 시작, 오류 처리 |
| 2 | `MainForm.cs` | 타이틀 바, 탭 전환, 종료 |
| 3 | `Core/VisionApp.cs` | **프로그램 중심.** 레시피 / 카메라 / 검사 1회(InspectOnce) / 카운트 / 기록 |
| 4 | `Core/JobRunner.cs` | 이미지 넣기 → Run → 판정 / Record / 결과 표 값 |
| 5 | `Core/ImageSources.cs`, `Core/CameraSources.cs` | File Mode 파일 읽기 / 카메라 연결·취득 |
| 6 | `Pages/*.cs` | 탭 화면 4개 (RUN / RECIPE / CONNECT CAMERA / SYSTEM) |
| - | `Core/Recipe.cs`, `SystemSettings.cs`, `ResultLogger.cs`, `ResultsAnalysisReader.cs` | 레시피 파일, 설정 파일, 기록, 결과 표 값 읽기 |
| - | `Core/VppInspector.cs`, `VppFile.cs` | vpp 바이트 확인, 안전한 저장 |
| - | `Theme.cs`, `Controls/*.cs` | 색상, 표시등, 카운트 카드 (동작과 관계없음) |

화면 배치는 각 `*.Designer.cs` 를 Visual Studio 디자이너로 연다.
Cognex 컨트롤(이미지 화면, ToolBlock 편집 화면)은 디자이너에 넣으면 깨지기 쉬워서 코드(`Init`)에서 넣는다.

## 6. 바꾸면 안 되는 것 (`[안전]` 표시)

| 내용 | 위치 | 바꾸면 생기는 일 |
|---|---|---|
| 바이트 확인 → 경고 → 불러오기 순서 | `VisionApp.LoadRecipe`, `ConnectCamera` | 경고 전에 카메라·I/O 에 연결됨 |
| 저장: 임시 파일 → 원본과 교체 → .bak | `VppFile.Save` | 저장 실패 시 원본 손상 |
| 레시피 정리 전 편집 화면 연결 끊기 | `VisionApp.UnloadRecipe` → `RecipePage.OnRecipeUnloading` | 정리 중 오류 |
| 종료 시 카메라 Disconnect | `VisionApp.ReleaseCameras` | R6025 오류창이 뜨고 멈춤 |
| 편집 화면 툴바의 열기/저장 숨김 | `RecipePage.HideFileButtons` | 백업 없이 저장됨 |
| 이미지 파일 Close | `FileImageSource.Next` | 파일이 잠긴 채로 남음 |

## 7. 저장소 주의

GitHub 공개 저장소다. 장비 정보·고객 정보·내부 경로를 문서에 적지 않는다. vpp·이미지는 `.gitignore` 로 빠진다.

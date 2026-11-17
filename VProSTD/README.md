# VProSTD

VisionPro 검사 프로그램. **이미지 취득과 검사(ToolBlock)를 분리**한 구조다.
VProEditAll 의 안전 규칙(바이트 확인 후 경고, 안전한 저장, 종료 시 카메라 해제)을 이어받았다.
작업 기록과 시험 결과는 `WORKLOG.md` 참고.

```
[이미지 입력]                                  [검사]                                  [결과]
 File Mode 켜짐 → 이미지 파일 / 폴더   ─┐
                                        ├→ ToolBlock.Inputs["InputImage"] → Run() → Outputs["Result"] → OK/NG, 카운트, Log, CSV, 이미지 저장
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

| ToolBlock 단자 | 형식 | 설명 |
|---|---|---|
| Inputs `InputImage` | ICogImage (또는 CogImage8Grey) | **필수.** 취득한 이미지가 들어온다. 흑백 입력에 컬러 이미지가 오면 자동으로 흑백 변환 |
| Outputs `Result` | bool | **필수.** true = OK, false = NG. 실행 오류는 NG 로 센다 |
| Inputs `PixelX`, `PixelY` | double | 선택. 있으면 SYSTEM 탭 값을 넣어 준다 |

- RECIPE 탭의 목록과 모든 버튼은 SYSTEM 탭의 **Recipe Path** 를 쓴다.
- RECIPE 탭의 NEW 는 위 필수 단자가 들어 있는 빈 ToolBlock 을 만든다.
- RECIPE 탭의 SAVE 는 Recipe Path 에서 파일 저장 창을 연다. 현재 레시피 파일 = 덮어쓰기, 다른 이름 = 다른 이름으로 저장 후 그 레시피로 바뀜. Recipe Path 밖에는 저장하지 않는다.
- RECIPE 탭의 DELETE 는 `<이름>.vpp`, `.vpp.bak`, `<이름>.recipe.xml` 만 지운다.
- 레시피 `.vpp` 에 카메라(AcqFifo)·I/O 설정이 들어 있으면 불러오기 전에 경고한다 (카메라는 CONNECT CAMERA 탭에서 따로 연결하는 것이 원칙).
- RUN 탭 아래 결과 표는 ToolBlock 안 **CogResultsAnalysisTool 의 Output 항목**(이름 = 열 제목)을 보여 준다.
  Output 항목의 값은 판정값(Accept/Reject)이다. CogResultsAnalysisTool 이 여러 개이면 `<이름>.recipe.xml` 의 `ResultsAnalysisToolName` 으로 지정한다.
- RUN 화면의 이미지는 `<이름>.recipe.xml` 의 `DisplayRecordName` Record, 없으면 이미지가 있는 첫 번째 Record, 그것도 없으면 입력 이미지.

## 3. 카메라 (CONNECT CAMERA 탭)

카메라는 레시피와 따로 관리한다. `.vpp` 를 불러오지 않고 코드에서 직접 연결한다 (`Core/CameraSources.cs`).

| 버튼 | 하는 일 |
|---|---|
| SEARCH | 연결된 카메라를 찾아 목록에 넣는다 (저장된 카메라를 골라 둠) |
| CONNECT | 목록에서 고른 카메라 → 없으면 저장된 카메라 → 없으면 첫 번째 카메라. 연결되면 현재 레시피 노출을 적용 |
| TRIGGER | 1장 취득해서 보여 줌 (검사 안 함) |
| 노출 | 바로 카메라에 적용. **레시피별 값**이므로 RECIPE 탭 SAVE 때 `<이름>.recipe.xml` 에 저장된다 |
| SAVE CAMERA | 연결한 카메라(시리얼, 영상 형식)를 `system.xml` 의 `Camera` 에 저장 |

- 레시피를 바꿔도 카메라 연결은 유지되고, 새 레시피의 노출이 적용된다 (레시피 노출이 0 이면 카메라 값 유지).
- 지금은 VisionPro 가 찾는 카메라(GigE)만 지원한다. Euresys CameraLink 에어리어 카메라는 `ICameraSource` 를 구현해서 `VisionApp.OpenCamera` 에 추가한다.

## 4. 설정과 기록 파일

| 파일 | 위치 | 내용 |
|---|---|---|
| `system.xml` | 실행 파일 옆 (고정) | SYSTEM 탭 값, 마지막 레시피 (다음 실행 때 자동으로 불러옴), 카메라 설정 (`Camera`) |
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

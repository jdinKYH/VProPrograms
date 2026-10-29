# VProManager02 작업 로그

## 2026-10-29 · VProManager01에서 분리

원본: `D:\10_InProcess\VINA\VProManager01` (커밋 `c2e40ec`, FHD 고정 배치까지)
복사 제외: `.git`, `bin`, `obj`, `.vs` · 이름 `VProManager01` → `VProManager02`, 프로젝트·솔루션·어셈블리 GUID 새로 생성

### 결정 사항 (사용자 확인 완료)

| # | 항목 | 결정 |
|---|---|---|
| 0 | 방향 | VPP(Job)는 QuickBuild에서 따로 검증한다. 프로그램은 툴·터미널 존재를 하나하나 확인하지 않는다. |
| 1 | 파일 모드 | VPP의 `tbImageSource` ToolBlock 스크립트로 해결. 입력 `IsFileMode`(int, 1=파일), `FileInputImage`. AcqFifo는 블록 안에 있고 라이브 모드에서만 실행. 프로그램은 값을 넣고 `ToolGroup.Run()` 한 번. |
| 2 | 취득 실패 | `tbImageSource` 결과가 Accept가 아니면 메시지만 표시하고 종료. 판정·카운트·CSV·이미지 저장 없음 (A안). |
| 3 | Locate | `Locate` 출력 `AlignScore` 사용 (B안). Error·값 없음·기준값 미만 → NG 기록(카운트·CSV·이미지). 측정값은 "-". |
| 3 | 측정 판정 | `leftDist01/02`, `rightDist01/02`를 Settings 창 Min/Max로 판정. 블록 결과가 Accept가 아니면 그 항목 NG. |
| 4 | 범위 | 설정 창, CSV, 폴더 일괄 검사·회귀 테스트, 로그, 마지막 VPP 자동 로드, FHD 화면 유지. |
| 5 | 결과 분석 Grid | 화면 하단. 열: 날짜, 모델(VPP 파일명), 결과 분석 툴 항목(툴 순서대로), 판정(빈칸, 나중에 추가), 이미지(경로). |
| 6 | 이미지 저장 | OK·NG 모두. 카메라 모드는 저장, 파일 모드는 복사하지 않고 원본 경로만 기록. |
| 7 | Folder Run | Grid에 행을 추가하지 않음 (기존처럼 Batch 보고서만). |

### VPP 트리 (tree3.png 기준)

```
<ToolGroup 1>
  Image Source            (사용 안 함)
  tbImageSource  in  FileInputImage, IsFileMode   (블록 안: CogAcqFifoTool1, 스크립트로 이미지 선택)
                 out OutputImage            → Locate.OutputImage
  Locate         out OutputImage            → LEFTside, RIGHTside
                 out AlignScore
  LEFTside       out leftDist01, leftDist02 → CogResultsAnalysisTool1.LeftD1, LeftD2
  RIGHTside      out rightDist01, rightDist02 → CogResultsAnalysisTool1.RightD1, RightD2
  CogResultsAnalysisTool1  (판정식 없음, Grid 표시용)
```

### 진행 내역

1. **프로젝트 생성**: VProManager01 복사 → 이름·GUID 변경 (`VProManager02.csproj`, `.sln`, 네임스페이스, 어셈블리)
2. **VisionService.cs 재작성 (단순 구조)**
   - 로드: ToolGroup만 확인, 툴은 이름(`tbImageSource`, `Locate`, `LEFTside`, `RIGHTside`)으로 찾기만 함. 결과 분석 툴은 형식으로 찾음
   - 실행: `Run(파일 모드, 이미지)` → `tbImageSource.Inputs["IsFileMode"]`=1/0, `["FileInputImage"]` 설정 후 `ToolGroup.Run()` 한 번
   - 확인: `GetImageSourceError()`(tbImageSource 결과), `CheckLocate()`(Locate Error / AlignScore 없음 / 기준값 미만), `EvaluateMeasure()`
   - 결과 분석: `GetAnalysisColumns()`(RunParams 키 순서), `GetAnalysisValues()`(EvaluatedExpressions, 없으면 입력 값)
   - 제거: 툴·터미널 검증, RunPipeline, 실행 여부 추적, 입력 대응표, 결과 레코드 대체 처리
3. **InspectionResult.cs**: 결과 분석 판정 제거, `ImagePath`·`AnalysisValues` 추가, CSV `Analysis` 열 → `ImagePath` 열, `SaveNgImage` → `SaveImage(이미지, 시각, OK여부)` (`Results\Images\yyyyMMdd\HHmmssfff_OK|NG.bmp`)
4. **InspectionSettings.cs / SettingsForm.cs**: `UseResultsAnalysisDecision`과 체크박스 제거, `SaveNgImage` → `SaveImage`, 기준 점수 라벨을 "Align 최소 점수 (AlignScore)"로
5. **BatchInspection.cs**: 보고서 `Analysis` 열 제거
6. **Form1.cs**: `runInspection`을 ①②③ 흐름으로 교체, 이미지 저장(카메라: 저장 / 파일: 원본 경로), 결과 Grid(열 생성·행 추가·최대 500행), 카메라 툴 경고 제거, 결과 화면을 Locate 출력 이미지 + Locate·LEFT·RIGHT 그래픽으로
7. **Form1.Designer.cs**: `gridResults`(DataGridView, x10 y696 1570x234) 추가, 이미지 화면 높이 884 → 640
8. **빌드**: 오류·경고 0

### 시험 (VPP `QB_Jobs\1029_QB_TbSrcResults01.vpp`, 파일 모드)

| 이미지 | 결과 |
|---|---|
| `261002_single\2BL.bmp` | OK, Align 1.00, 측정 648.00/648.45/624.01/648.45, 결과 분석 5개 값 표시 |
| `261002_single\3TI.bmp` | NG, Align 0.56 < 0.6 (FAIL), 측정·분석 "-" |
| `261002_single\1cal.bmp`, `261001\img-8cvt\*.BMP` 6장 | NG, Locate Error (PMAlign 결과 0개 → CogFixtureTool1 실패) |

- 결과 Grid 열: `날짜 | 모델 | LeftD1 | LeftD2 | RightD1 | RightD2 | Output | 판정 | 이미지` (Output은 툴에 있는 계산식)
- CSV에 ImagePath 열 기록 확인, 카메라 모드 이미지 저장 함수(`SaveImage`) 단독 호출로 `_OK.bmp`/`_NG.bmp` 저장 확인 (한 장 약 5MB)
- 시험 데이터(`C:\ProgramData\VProManager02`)는 삭제함
- **시험 중 수정**: Locate가 Error인데 LEFT/RIGHT는 Accept로 값을 내는 경우가 있어, 위치 확인 실패 행은 결과 분석 칸도 "-"로 표시하도록 변경

### 확인 필요 (VPP)

- Locate가 대상을 못 찾아도 출력 `AlignScore`에 이전 값(0.99997)이 남아 있음 → 못 찾으면 0을 내보내도록 Locate 스크립트 수정 필요 (프로그램은 Locate Error를 먼저 확인하므로 현재 판정은 맞음)
- 카메라 모드는 시험하지 않음 (카메라 연결 필요)
- 이미지 화면(CogRecordDisplay)은 캡처 방식으로 그려지지 않아 화면 표시는 실제 실행에서 확인 필요

## 2026-10-29 · Locate 스크립트 (AlignScore 이전 값 문제 해결)

- 문제: PMAlign이 대상을 못 찾아도 `Locate.AlignScore`에 이전 값(0.99997)이 남고, Fixture가 `Results[0].GetPose()` 바인딩 오류로 Error
- 해결 (Locate ToolBlock 스크립트, QuickBuild 컴파일 성공):
  1. 실행 전 `AlignScore` = 0
  2. 블록 안 툴을 순서대로 실행, PMAlign 결과가 0개면 멈춤 (Fixture 실행 안 함) → 결과 Reject, 메시지 "대상을 찾지 못했습니다"
  3. 찾으면 `AlignScore` = `Results[0].Score`
- 프로그램 표시: 못 찾음·점수 부족 모두 `x.xx - FAIL` → NG (프로그램 수정 없음)
- 이 VisionPro 버전의 ToolBlock 스크립트 작성 규칙 (시행착오로 확인)
  - `mToolBlock` → 보호 수준 오류, `Tools.이름` → 없음, `Outputs.이름` → Locate에서는 없음
  - 사용할 것: `private CogToolBlock block;` + `Initialize`에서 `block = (CogToolBlock)host;`
    → `block.Tools["이름"]`, `block.RunTool(...)`, `block.Inputs["이름"].Value`, `block.Outputs["이름"].Value`
  - 블록 안 툴을 스크립트에서 실행했으면 `GroupRun`은 `return false`

## 2026-10-29 · 카메라 동시 사용 불가 확인

- 증상: QuickBuild에서 `User Script (0:0): Tool "CogAcqFifoTool1" failed. 이제 프레임 그래버가 없습니다.`
- 원인: GigE 카메라는 한 프로세스만 제어 가능. VProManager02(또는 같은 VPP를 로드한 다른 프로세스)가 카메라를 잡고 있으면 QuickBuild에서 취득 불가, 반대도 같음
- 확인: QuickBuild를 종료하니 프로그램 정상 (사용자 확인)
- 참고: AcqFifo가 tbImageSource 안에 있으므로 **VPP를 로드하는 순간** 카메라가 연결된다. 파일 모드로만 써도 카메라를 잡고 있음
- 운영: QuickBuild와 VProManager02를 동시에 실행하지 않는다. 꼬이면 카메라를 잡은 쪽을 완전히 종료 후 다른 쪽 실행
- 시험 시 주의: 카메라가 들어 있는 VPP를 별도 프로세스에서 로드하면 사용 중인 QuickBuild의 카메라 연결이 끊길 수 있음 (오늘 시험 중 발생했을 가능성)

## 2026-10-29 · 위치 확인 실패 시 Result Image가 남는 문제

- 증상 (사용자 화면 locateNG2/locateOK/locateNG): OK 검사 다음 위치 확인 실패(NG) 검사에서 Result Image에 이전 검사 이미지가 남음 (그래픽만 지워짐). NG 다음 NG는 비어 있어 드러나지 않았음
- 원인: 검사 시작 때 `clearDisplayAll()`의 `Image = null`이 화면에 반영되지 않는 경우가 있고, 위치 확인 실패 시에는 Result 화면에 아무것도 넣지 않았음
- 수정: 위치 확인 실패 시 `showLocateFailDisplay()` — 이번 입력 이미지 + 빨간 글자 "위치 확인 실패 (0.56 - FAIL)" (비우지 않고 이번 이미지로 교체)
- 함께 수정
  - 결과 화면 왼쪽 위 녹색 그래프·깨진 글자 제거: 캘리퍼 `RegionData` 레코드(ProjectionGraph, FilteredProjectionGraph, FoundEdges — 영역 이미지의 픽셀 좌표 `#`)를 그리지 않음. 그래픽 목록을 뽑아 원인 확인
  - 검사 후 포커스를 Run 버튼으로 (Left Dist1 칸이 선택돼 보이던 문제)
- 유지: 위치 확인 실패 메시지창 (사용자 결정), Result 라벨(하늘색 긴 숫자 등)은 사용자가 VPP에서 정리
- 시험 (QuickBuild·VProManager02 종료 후, `1029_QB_TbSrcResults01.vpp`, 실제 화면 캡처)
  - 2BL(OK): 측정 그래픽 정상, 왼쪽 위 그래프·깨진 글자 없음
  - 이어서 3TI(NG 0.56): Result Image가 3TI 이미지 + "위치 확인 실패 (0.56 - FAIL)"로 바뀜
  - 포커스 변경은 화면 시험에서 확인하지 못함 (시험 방식이 Run 버튼을 거치지 않음)

## 2026-10-29 · 결과 Grid 선택 표시

- 증상: 새 행을 맨 위에 넣어도 선택 표시(파란 줄)가 이전 행에 남아 이전 검사가 현재 결과처럼 보임
- 수정: `addResultGridRow`에서 행 추가 후 `ClearSelection()`, `CurrentCell = null`, 맨 위로 스크롤
- 시험: VPP 로드·창 표시 없이(카메라 연결 안 함) 행 3개 추가 → 매번 맨 위가 새 행, 선택된 행 0개

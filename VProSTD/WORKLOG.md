# VProSTD WORKLOG

VProEditAll 을 바탕으로 만든 검사 프로그램. 이미지 취득(카메라, 코드로 연결)과 검사(레시피 .vpp = ToolBlock)를 분리.

## 2026-11-17 레시피 파일 단위 변경, 카메라 코드 연결

### 결정 사항
| # | 항목 | 적용 |
|---|---|---|
| 1 | 레시피 단위 | 폴더(Job.vpp + Acq.vpp + recipe.xml) → **사용자가 이름 붙인 `.vpp` 파일** + `<이름>.recipe.xml` |
| 2 | 카메라 | Acq.vpp 불러오기 삭제. `CameraSettings`(system.xml) 값으로 코드에서 직접 AcqFifo 생성 (`CognexCameraSource`) |
| 3 | 카메라 관리 | 레시피와 분리. CONNECT CAMERA 탭 SEARCH / 카메라 선택 / CONNECT / SAVE CAMERA. 레시피를 바꿔도 연결 유지 |
| 4 | 노출 | 레시피별 (`RecipeSettings.ExposureMs`). 레시피 불러오기 / 카메라 연결 때 적용 |
| 5 | 카메라 종류 | `ICameraSource` 인터페이스. 지금은 VisionPro 카메라(GigE), 나중에 Euresys CameraLink 에어리어 추가 |
| 6 | 트리거 | 소프트웨어 트리거 (Manual, `Acquire`) |

### 확인 결과 (카메라 없이)
- 빌드: 오류·경고 없음
- 레시피 목록: Recipe Path 의 ToolBlock `.vpp` 만 나옴. CogAcqFifoTool `.vpp` 는 빠짐
- NEW / COPY / DELETE(`.vpp`·`.bak`·설정 파일만 삭제), 중복 이름 거부, `.vpp` 붙여 입력해도 처리
- 레시피 노출 저장/읽기, 카메라 항목이 없는 기존 system.xml 읽기 (기본값으로 채움)

### 아직 확인하지 못한 것
- SEARCH / CONNECT / TRIGGER / 노출 변경 / SAVE CAMERA / 레시피 바꿀 때 노출 적용 / 종료 시 카메라 해제 (장비 PC)
- 화면: RECIPE 탭 SAVE 파일 창, CONNECT CAMERA 탭 새 버튼 배치

## 2026-11-17 1~8단계 구현

### 결정 사항
| # | 항목 | 적용 |
|---|---|---|
| 1 | 이미지 전달 | 메모리 이미지를 바로 `Inputs["InputImage"]` 에 넣음. 파일 저장은 SYSTEM 탭 (Origin / Result, BMP·JPG) |
| 2 | 편집 | RECIPE 탭 = ToolBlock 편집, CONNECT CAMERA 탭 = 취득 (노출) |
| 3 | 판정 | `Outputs["Result"]` (bool). 실행 오류는 NG 로 집계, Log 에 ERROR |
| 4 | 연속 실행 | 소프트웨어 반복 (UI 스레드 + Timer) |
| 5 | 관리 단위 | 레시피 폴더 (Job.vpp + Acq.vpp + recipe.xml) |
| 6 | 화면 | FHD 1920x1080 고정, 기본 제목줄 없음, 다크 테마, 카운트 카드, 선택 탭 파랑 |
| 7 | 결과 표 | CogResultsAnalysisTool 의 Output 항목만, 3줄 보이고 스크롤, 최신이 위, 최대 1000줄 |
| 8 | 설정 파일 형식 | XML (`system.xml`, `recipe.xml`). .NET Framework 기본 기능(XmlSerializer)만 사용하려고 json 대신 선택 |

### 확인 결과 (카메라 없이, scratchpad 사본 폴더에서 시험)
- 빌드: 오류·경고 없음
- Core 시험 (시험용 ToolBlock 과 이미지를 코드로 만들어 사용, 31항목 모두 통과)
  - NEW(InputImage/Result 자동 생성) / LOAD / SAVE(.bak 생성) / COPY(.bak 제외) / DELETE, 잘못된 이름·중복 이름 거부
  - File Mode 폴더 검사: 숫자 순서 정렬(img1, img2, img3, img10), 24bit 컬러 bmp → 흑백 입력 자동 변환, 읽을 수 없는 파일은 ERROR(NG) 로 기록 후 다음 파일
  - 결과 표: Output 항목만 읽음 (Input 항목 제외). CogResultsAnalysisTool 에는 기본 `Output` 항목이 항상 들어 있음
  - CSV: 오류 줄이 있어도 머리글이 한 번만 써짐 (시험 중 발견한 버그 수정)
  - 다시 불러오면 File Mode 와 이미지 폴더가 복원됨
  - 카메라 모드에서 CONNECT 없이 실행 → ERROR 결과
- 화면 시험 (MainForm 을 화면 밖에 띄워 버튼 동작 호출)
  - 시작 시 마지막 레시피 자동 불러오기, RUN ONCE 4회 → 카운트 4/3/1, 결과 표 4줄 5열
  - AUTO RUN: 폴더 끝에서 자동 정지, 나쁜 파일은 건너뜀, 실행 중 탭 이동 차단, STOP 정지
  - Origin / Result 이미지 저장 확인 (Result 는 이미지 화면을 그림으로 저장)
  - RECIPE 탭 ToolBlock 편집 화면 표시

### 아직 확인하지 못한 것 (장비 PC 에서 사본으로 확인 필요)
- CONNECT / TRIGGER / 노출 변경 / 카메라 AUTO RUN / 연결 끊김 시 자동 정지 / 종료 시 카메라 해제
- Acq.vpp 가 없을 때 첫 번째 카메라로 새로 만들기 → SAVE → 다시 CONNECT
- 실제 검사 ToolBlock (이미지 툴 + CogResultsAnalysisTool) 의 Record 표시, Result 이미지 그래픽
- InputImage 가 안쪽 툴과 연결되지 않은 ToolBlock 경고 (계획에 있었으나 이번에는 구현하지 않음)
- PLC 연결 (표시등만 있음)

## 2026-11-17 화면·저장 방식 수정

| # | 내용 |
|---|---|
| 1 | RUN 탭: NG COUNT 카드와 YIELD 카드 위치 교환 |
| 2 | RECIPE 탭: 목록·버튼 모두 Recipe Path 사용 (SYSTEM SAVE 로 경로가 바뀌면 목록 다시 읽음) |
| 3 | RECIPE SAVE: Recipe Path 에서 폴더 선택 → 현재 폴더면 덮어쓰기, 다른 폴더면 다른 이름으로 저장 후 전환. 저장 실패 시 원래 레시피로 되돌림 |
| 4 | SYSTEM LOAD / SAVE: System Path 에서 파일 선택. SAVE 는 고른 파일 + 실행 파일 옆 system.xml 에 함께 저장 |
| 5 | SYSTEM: 그룹 이름 "Pixel Size (mm/pixel)" |

확인: 빌드 오류·경고 없음, Core 시험 37항목 통과 (다른 이름으로 저장, 설정 파일 저장·읽기 추가), 화면 시험 통과.
파일 선택 창(폴더/파일 고르기)은 사람이 눌러야 해서 자동 시험에서는 확인하지 못함.

## 2026-11-19 Live Mode, 레시피 단자 경고 창

| # | 내용 |
|---|---|
| 1 | CONNECT CAMERA 탭: TRIGGER 아래 LIVE 버튼. 누르면 LIVE ON (파랑, 30 ms 타이머로 계속 1장씩 취득해 표시), 다시 누르면 LIVE OFF |
| 2 | LIVE ON 중에는 TRIGGER / SEARCH / 카메라 선택 / CONNECT / File Mode 막음. 노출은 바꿀 수 있음 (화면으로 바로 확인) |
| 3 | LIVE 자동 OFF: 다른 탭으로 이동, 카메라 연결 끊김, 취득 실패 (Log 에 NG 한 줄), 프로그램 종료 |
| 4 | Live 첫 장만 화면에 맞추고, 그 뒤에는 사용자가 확대한 상태 유지 |
| 5 | 레시피 불러올 때 Inputs `InputImage` 또는 Outputs `Result` 가 없으면 경고 창 (레시피는 불러온 상태로 둠, 시작 시 자동 불러오기 때도 뜸) |

확인: 컴파일 오류 없음 (실행 중인 프로그램이 exe 를 잡고 있어 bin 복사는 실패 → 프로그램 종료 후 다시 빌드 필요), 화면 배치 확인.
아직 확인하지 못한 것: 장비 PC 에서 LIVE ON/OFF, Live 중 노출 변경, Live 중 탭 이동/종료, 경고 창.

## 2026-11-19 RUN 결과 이미지가 안 보이던 문제

- 원인: 레시피 Outputs 에 `Result` 가 없으면 `Outputs["Result"]` 에서 예외 → 검사 결과가 화면까지 오지 못함 (Log: "Collection does not contain an index for Result")
- 수정 1: `Result` 가 없으면 ERROR(NG) 로 판정하되, 결과 이미지·결과 표는 그대로 만든다
- 수정 2: 기본 표시 Record 를 "이미지가 있는 첫 번째 Record(= 입력 이미지)" → "이미지가 있는 마지막 Record(= 마지막 툴 결과)" 로 변경. recipe.xml `DisplayRecordName` 을 쓰면 그 Record 우선
- 확인: 사본 레시피 + 시험 이미지로 JobRunner.Run 호출 → ERROR 판정 + `CogFixtureTool1.OutputImage` 표시, DisplayRecordName 지정 시 그 Record

## 2026-11-19 판정을 CogResultsAnalysisTool 로

| # | 내용 |
|---|---|
| 1 | 판정: CogResultsAnalysisTool 의 Decision. Accept = OK, Error = ERROR, Warning/Reject = NG |
| 2 | CogResultsAnalysisTool 이 없으면 예전처럼 Outputs `Result` (bool) 로 판정. 둘 다 없으면 ERROR (이미지·표는 표시) |
| 3 | 결과 표·CSV: `Time | Judge | Output 항목들` (Output 종류 항목만, 변경 없음) |
| 4 | 툴 찾기: 최상위 ToolBlock 의 툴을 먼저, 없으면 안쪽 ToolBlock. recipe.xml `ResultsAnalysisToolName` 이 있으면 그 이름 |
| 5 | 레시피 불러올 때 경고: "Result 없음" → "판정할 CogResultsAnalysisTool 없음 (Result 도 없음)" |

확인 (사본 레시피 + 시험 이미지): 1119 Block → OK, 표 `Output = Accept`, 경고 없음. Locate1 → 경고 + ERROR, 이미지 표시.
Reject/Warning → NG 는 시험 레시피가 없어 확인하지 못함. NEW 레시피는 지금도 Outputs `Result` 를 만든다 (CogResultsAnalysisTool 을 추가하면 그쪽이 우선).

## 2026-11-19 결과 표에 Input 항목 추가

- 결과 표·CSV: `Time | Judge | CogResultsAnalysisTool 의 Input 항목 + Output 항목` (툴 안 순서, 중간 계산식 Expression 은 제외)
- 확인 (사본 1119 Block + 시험 이미지): `LeftD1 647.96 | LeftD2 648.414 | RightD1 624.005 | RightD2 648.45 | Output Accept`, 판정 OK
- CSV 는 열이 바뀌면 머리글을 한 줄 다시 쓴다 (기존 동작)

## 2026-11-19 RUN 화면에 이전 검사 이미지가 나오던 문제

- 원인: "이미지가 있는 마지막 Record" 를 자동으로 고르는데, 툴이 실패하면 고르는 Record 가 바뀌고 (예: `LEFTside.CogDistancePointPointTool1.InputImage`),
  실행되지 못한 툴의 Record 에는 이전 검사 이미지·그래픽이 남아 있음. ERROR 일 때 결과 표 값도 실패한 툴의 이전 출력값이었음
- 수정 1: RECIPE 탭 DELETE 아래 "RUN 표시 이미지" 목록 (`(입력 이미지)` + ToolBlock 의 이미지 Record). 고르면 바로 적용, 레시피 SAVE 때 recipe.xml `DisplayRecordName` 에 저장
- 수정 2: 이름이 비어 있거나 Record 가 없으면 입력 이미지. 고른 Record 의 툴이 이번 검사에서 ERROR 이면 입력 이미지 (이전 이미지 방지)
- 수정 3: ERROR 이면 결과 표·CSV 값 칸을 비움 (열은 유지)
- 확인 (사본 1119 Block, 2BL → 3TI): `Locate.CogFixtureTool1.OutputImage` 선택 시 두 경우 모두 그 Record (3TI 는 Locate 가 정상이라 이번 이미지), 3TI 값 빈 칸, 빈 이름/없는 이름은 입력 이미지. 화면 배치 확인
- 미확인: 실제 프로그램에서 목록 열기·선택·SAVE 후 다시 불러오기 (실행 중이라 bin 복사 실패)

## 2026-11-19 앞단 툴 실패 시 거짓 OK 막기

- 발견 (사본 1119 Block 에 빈 이미지): PMAlign 결과 0개여도 RunStatus Accept, Fixture 도 Accept 인데 OutputImage 는 이전 이미지.
  Locate 는 Reject 지만 LEFTside/RIGHTside 가 이전 이미지로 측정 → CogResultsAnalysisTool1 Accept → VProSTD 판정 OK (거짓 OK)
- Fixture OutputImage 는 읽기 전용. InputImage=null 로 실행해도 OutputImage 는 지워지지 않음 (스크립트로 직접 삭제 불가)
- 수정 1 (판정): 판정 툴을 뺀 최상위 툴 중 Reject/Error 가 있거나 ToolBlock 이 Reject/Error 이면 NG. Log 에 "실패한 툴: Locate Reject"
- 수정 2 (표시): RUN 표시 이미지 Record 까지 가는 툴(Locate → CogFixtureTool1) 중 하나라도 Reject/Error 이면 입력 이미지
- 수정 3 (값): ERROR 이거나 앞단 툴이 실패하면 값 비움. 판정 툴만 Reject (기준 벗어남) 이면 값 유지
- 확인 (사본, 2BL → 빈 이미지 → 3TI → 2BL): OK / NG·입력 이미지·값 빈 칸 / ERROR·값 빈 칸 / OK
- 다음: Locate ToolBlock 스크립트로 PMAlign 실패 시 출력 OutputImage 를 비우는 방법을 사본에서 시험 (return false 후 출력 연결이 다시 적용되는지 미확인)

## 2026-11-19 RUN 표시 이미지 단순화 (사용자 요청)

- RUN 화면은 검사 후 항상 `Locate.CogFixtureTool1.OutputImage` 를 보여 준다 (`JobRunner.DisplayRecordName` 상수 하나)
- 삭제: RECIPE 탭 "RUN 표시 이미지" 목록, recipe.xml `DisplayRecordName`, 툴 실패에 따라 이미지를 바꾸던 조건
- 그 Record 가 없는 ToolBlock (예: Locate1) 만 입력 이미지
- 판정·값 비우기 (앞단 툴 실패 → NG) 는 그대로
- 확인 (사본, 2BL → 빈 이미지 → 3TI → 2BL): 모두 Locate.CogFixtureTool1.OutputImage 표시, 판정 OK/NG/ERROR/OK. 예전 recipe.xml (DisplayRecordName 들어 있음) 읽기 정상

## 2026-11-19 Locate 스크립트 시험 (사본, 원본 레시피 변경 없음)

- 기존 Locate 스크립트: PMAlign 못 찾으면 Fixture 를 건너뛰고 Reject. 그러나 Fixture.OutputImage 와 Locate 출력 OutputImage 에 이전 이미지가 남음
- `block.Outputs["OutputImage"].Value = null` 은 효과 있음 (return false 뒤 출력 연결이 다시 적용되지 않음) → LEFTside 는 입력 없음 Error
- Fixture 를 이번 이미지로 다시 실행하는 것은 PMAlign→Fixture 바인딩(`RunParams.UnfixturedFromFixturedTransform <= Results.Item[0].GetPose()`) 때문에 실패 (fixture.Run() 도 바인딩 적용)
- 해결: 그 바인딩을 지우고 스크립트에서 Pose 를 직접 넣음. 못 찾으면 변환 없이(CogTransform2DLinear) Fixture 실행 → Fixture.OutputImage = 이번 이미지
- 확인 (2BL → 빈 이미지 → 3TI → 2BL): 2BL 값·AlignScore 기존과 동일, 빈 이미지에서 Fixture.OutputImage = 이번 이미지, Locate 출력 null, 판정 ERROR(LEFTside 입력 없음)
- 참고: RIGHTside 스크립트는 입력 이미지가 없어도 Accept (툴 결과를 result 에 반영하지 않음)

## 2026-11-19 RIGHTside 스크립트 확인 (사본)

- 정정: 앞에서 본 "RIGHTside = Accept" 는 RIGHTside 가 실행되지 않아 이전 상태가 남은 것.
  최상위 ToolBlock 은 툴 하나가 Error 이면 그 뒤 툴(RIGHTside, CogResultsAnalysisTool1)을 실행하지 않음 (RunStatus 객체가 바뀌지 않음으로 확인)
- RIGHTside 만 따로 실행: 원래 스크립트도 Caliper 실패 시 Error 를 내고 그래픽도 그리지 않음 (정상).
  문제는 출력 rightDist01/02 에 이전 검사 값이 남는 것
- 수정안: 시작할 때 출력 0, 툴 실패 시 그 자리에서 멈춤, 성공 시 mDist.Distance 를 출력에 직접 넣음 → 실패 시 0, 정상 값 동일 확인
- LEFTside 도 같은 구조 (leftDist01/02, Width)

## 2026-11-19 LEFTside 스크립트 시험 (사본)

- Width 출력은 스크립트가 아니라 출력 바인딩으로 들어옴. Caliper 는 Edge 를 못 찾아도 Accept(결과 0개) → 기존 스크립트는 mC3.Results[0] 예외로 Error 가 됐던 것
- 수정안: 시작할 때 출력 0, 툴 Reject/Error 또는 Caliper 결과 0개면 그 자리에서 멈춤(Reject "CogCaliperTool1 실패: Edge 를 찾지 못함"), 성공 시 leftDist01/02·Width 직접 출력
- LEFTside 단독: 정상 값 동일, 3TI·빈 이미지·입력 없음 모두 출력 0·그래픽 없음
- 전체 (Locate·LEFTside·RIGHTside 수정): 2BL OK / 빈 이미지 ERROR (Locate Reject, LEFTside 입력 없음) / 3TI ERROR (LEFTside Reject 뒤 RIGHTside 도 실행되어 Edge 실패 Error) / 2BL OK
- 최상위 ToolBlock 은 Error 에서만 뒤 툴을 멈추고 Reject 는 계속 실행함 (3TI 에서 RIGHTside 가 실행된 것으로 확인)

## 2026-11-19 RIGHTside Caliper 확인 시험 (사본)

- RIGHTside 에 Caliper 결과 0개 확인을 넣었더니 3TI 에서 Reject 뒤에 출력 바인딩(rightDist01/02 ← Caliper Results.Item[0].Width) 실패로 Error 로 덮임
- 출력 바인딩은 스크립트 뒤에, 이번에 실행된 툴에 대해서만 적용되는 것으로 보임 (LEFTside 3TI 는 바인딩 원천 툴이 실행 전에 멈춰서 영향 없었음)
- 해결: LEFTside(3개)·RIGHTside(2개) 출력 바인딩을 지우고 스크립트에서 직접 출력. RIGHTside 는 원래 바인딩과 같은 Caliper Width 를 넣음
- 결과 (Locate·LEFTside·RIGHTside 수정 + 출력 바인딩 삭제): 2BL OK (값 동일) / 빈 이미지 ERROR / 3TI NG "실패한 툴: LEFTside Reject, RIGHTside Reject" / 2BL OK
- CogResultsAnalysisTool1 은 판정 기준을 아직 받지 못해 일부러 기준 없이 둔 상태 (항상 Accept). 앞단 실패는 VProSTD 가 NG 처리

## 2026-11-19 Locate·LEFTside·RIGHTside 스크립트 보완 (사본 시험, 원본 레시피 변경 없음)

- Locate 는 간단 스크립트 (편집 화면의 사용자 부분 + 자동 생성 부분). 사용자 부분만 바꿔야 컴파일됨
- Locate: PMAlign Error 는 원래 오류 유지, 못 찾으면 Fixture 를 변환 없이 실행한 "뒤에" 출력 OutputImage 비움 (Fixture 실행 시 출력 링크가 바로 적용됨)
- LEFTside/RIGHTside: 입력 이미지 없으면 Reject (Error 면 바깥 ToolBlock 이 뒤 툴을 멈춰 이전 상태가 남음)
- 결과: 2BL OK / 빈 이미지 NG "Locate·LEFTside·RIGHTside Reject" / 3TI NG "LEFTside·RIGHTside Reject" / 2BL OK. 네 경우 모두 모든 툴 실행됨
- 레시피 작업: Fixture 위치 링크 1개, LEFTside 출력 링크 3개, RIGHTside 출력 링크 2개 삭제 + 스크립트 교체

## 2026-11-19 RECIPE 탭 실행 시 "입력 이미지 없음"

- 원인: ToolBlock Inputs["InputImage"] 를 채우는 곳이 검사(JobRunner.Run) 뿐. 레시피 로드 직후 입력은 비어 있음 (.vpp 에서 되살아나지 않음, 사본으로 확인)
- 수정: JobRunner.SetInputImage (이미지 + PixelX/PixelY, 흑백 변환), VisionApp.SetRecipeImage.
  CONNECT CAMERA 탭에서 Single Image / Image Folder 고를 때(첫 이미지), TRIGGER 할 때 입력에 넣음. 레시피 로드 때는 넣지 않음 (사용자 결정). Live 는 넣지 않음
- 확인 (사본): 로드 직후 바로 실행 → "제공된 입력 이미지가 없습니다", SetInputImage 후 바로 실행 → Accept

## 2026-11-20 SAVE CAMERA 노출 저장

- 증상 (장비 PC): 노출 20→10, SAVE CAMERA, 재실행 → 노출 칸 10, CONNECT·LIVE 후 20 으로 돌아감
- 원인: SAVE CAMERA 는 시리얼·영상 형식만 저장하고 노출은 RECIPE SAVE 때만 저장됨.
  재실행 때 보인 10 은 노출 칸의 화면 초기값 (저장된 값 아님). CONNECT 하면 카메라 실제 값으로 바뀜 (LIVE 는 노출을 바꾸지 않음)
- 수정 (사용자 결정): SAVE CAMERA 때 레시피를 불러왔으면 지금 노출을 <이름>.recipe.xml 에도 저장 (.vpp 는 저장 안 함). 노출은 계속 레시피별
- 연결 전 노출 칸: 레시피 노출(있으면) 표시, 없으면 빈칸. 레시피가 바뀌면 다시 표시
- 장비 PC 확인 필요: 노출 변경 → SAVE CAMERA → 재실행 → 노출 칸 값 → CONNECT 후 같은 값 유지

## 2026-11-20 레시피 SAVE 를 Minimum 으로

- All 로 저장하면 툴 14개의 마지막 입력 이미지까지 들어가 .vpp 가 약 15MB (원래 약 5MB)
- 사본 시험: Minimum 저장 4.95MB, PMAlign 학습 패턴·학습 이미지 유지, 스크립트 오류 없음,
  2BL OK / 빈 이미지 NG / 3TI NG / 2BL OK, 측정값 All 과 동일
- 참고: 같은 이미지 객체를 서로 다른 두 ToolBlock 에 넣으면 Fixture "만들지 않은 좌표 공간을 수정" 오류 (시험 방법 문제, 프로그램은 매번 새 이미지)
- 수정: VisionApp.SaveRecipe (다른 이름으로 저장 포함) 를 Minimum 으로. 불러온 직후 편집 화면에는 이전 결과 그래픽이 없음 (한 번 실행하면 보임)

## 2026-11-20 종료 시 카메라 해제 로그

- 장비 PC 확인 (사용자): SEARCH, TRIGGER, 레시피 바꿀 때 노출 적용 정상
- 종료 시 해제 결과가 로그에 없어 확인이 어려움 → ReleaseCameras 가 해제를 모두 끝낸 뒤 로그를 남김
  - "CAMERA: 해제 완료 (n 대)" / "CAMERA: 해제 n 대, 실패 - ..." / 카메라를 안 쓴 실행은 "해제할 것 없음"
  - 한 대가 실패해도 나머지는 계속 해제. 로그 쓰기 오류는 무시 (해제를 막지 않게)
- 장비 PC 확인: Live OFF 종료·Live ON 종료 모두 "해제 완료 (1 대)" (종료 후 약 0.1초), 다시 실행 후 CONNECT 정상

## 2026-11-20 시작 시 저장된 카메라 자동 연결, 사용자 매뉴얼 초안

- 사용자 결정: 시작할 때 저장된 카메라에 연결하는 것을 기본으로, 카메라 선택·연결은 엔지니어 담당
- MainForm_Shown: 마지막 레시피 불러오기 → VisionApp.ConnectSavedCamera (확인 창 없음, 레시피 노출 적용)
  - system.xml Camera.Serial 이 비어 있으면 연결하지 않음 (주황 로그), Camera.AutoConnect = false 면 끔 (기본 true, 화면 설정 없음)
  - 실패하면 빨강 로그 + 경고 창 (작업자가 알 수 있게)
  - CONNECT 버튼과 같은 열기 함수(OpenAndApply) 사용
- 매뉴얼: `메뉴얼/VProSTD_사용자_메뉴얼.md` v0.1 (1부 작업자 / 2부 엔지니어 / 3부 문제 해결, 그림 자리만)
- 매뉴얼 Word 판: `메뉴얼/VProSTD_사용자_메뉴얼.docx` (A4 16쪽, 표지·목차·머리글·쪽 번호). Markdown 을 원본으로 변환 (변환 스크립트는 저장소 밖), 목차는 Word 에서 갱신 후 저장
- 매뉴얼 HTML 판: `메뉴얼/VProSTD_사용자_메뉴얼.html` (파일 1개, 외부 파일·인터넷 없이 열림). 왼쪽 목차(지금 읽는 장 표시), 검색(Enter 다음 / Shift+Enter 이전, Ctrl+F),
  좁은 화면은 목차 버튼, 브라우저가 어두운 모드면 어두운 색, 인쇄 시 부마다 새 쪽. Edge 로 넓은 화면·375px·인쇄·검색 확인
- 변환 스크립트: `메뉴얼/tools/` (md2html.js 는 추가 설치 없음, md2docx.js 는 `npm install` 후). Markdown 이 원본, 고친 뒤 `npm run html` / `npm run docx`

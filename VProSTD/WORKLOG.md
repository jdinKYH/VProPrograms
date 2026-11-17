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

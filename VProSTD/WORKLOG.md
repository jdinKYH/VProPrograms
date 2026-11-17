# VProSTD WORKLOG

VProEditAll 을 바탕으로 만든 검사 프로그램. 이미지 취득(Acq.vpp)과 검사(Job.vpp = ToolBlock)를 분리.

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

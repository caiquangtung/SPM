## **Tài liệu Đặc tả Yêu cầu Phần mềm (SRS)**

### **Hệ thống Web Quản lý Dự án và Theo dõi Tiến độ Thông minh**

**Phiên bản:** 1.0

**Ngày:** 28/10/2025

---

### **1. Giới thiệu**

### **1.1. Mục đích**

Tài liệu này nhằm mục đích mô tả chi tiết các yêu cầu chức năng và phi chức năng cho "Hệ thống Web Quản lý Dự án và Theo dõi Tiến độ Thông minh". Tài liệu sẽ là cơ sở để đội ngũ phát triển (Developers), kiểm thử (Testers), quản lý dự án (Project Managers) và các bên liên quan có cùng một sự hiểu biết về sản phẩm cần xây dựng.

### **1.2. Phạm vi sản phẩm**

Sản phẩm là một ứng dụng web (web application) giúp các nhóm phát triển phần mềm và doanh nghiệp vừa/nhỏ quản lý dự án một cách hiệu quả.

Điểm khác biệt cốt lõi của hệ thống là việc tích hợp một **Trợ lý AI Thông minh**, sử dụng Mô hình Ngôn ngữ Lớn (LLM) và kỹ thuật RAG (Retrieval-Augmented Generation). Trợ lý này cho phép người dùng tương tác với dữ liệu dự án bằng ngôn ngữ tự nhiên, tự động tạo báo cáo, tóm tắt thông tin và đưa ra các cảnh báo sớm, giúp người quản lý đưa ra quyết định nhanh chóng và chính xác.

### **1.3. Định nghĩa và từ viết tắt**

- **SRS:** Software Requirements Specification - Đặc tả Yêu cầu Phần mềm.
- **PM:** Project Manager - Người Quản lý Dự án.
- **LLM:** Large Language Model - Mô hình Ngôn ngữ Lớn.
- **RAG:** Retrieval-Augmented Generation - Kỹ thuật kết hợp truy xuất thông tin và sinh văn bản.
- **UI:** User Interface - Giao diện người dùng.
- **API:** Application Programming Interface - Giao diện lập trình ứng dụng.
- **CI/CD:** Continuous Integration/Continuous Deployment - Tích hợp liên tục/Triển khai liên tục.
- **Task:** Một công việc hoặc một đơn vị công việc cần hoàn thành.
- **VectorDB:** Vector Database - Cơ sở dữ liệu vector (sử dụng pgvector extension cho PostgreSQL).
- **pgvector:** PostgreSQL extension cho phép lưu trữ và tìm kiếm vector embeddings.

### **1.4. Tổng quan tài liệu**

Tài liệu này bao gồm các phần: Mô tả tổng quan về sản phẩm, các yêu cầu chức năng chi tiết theo từng module, các yêu cầu phi chức năng (hiệu năng, bảo mật,...), và các yêu cầu về giao diện.

---

### **2. Mô tả tổng quan**

### **2.1. Bối cảnh sản phẩm**

Sản phẩm là một hệ thống độc lập, được xây dựng từ đầu, hướng tới việc cạnh tranh với các công cụ quản lý dự án hiện có như Jira, Asana, Trello bằng cách cung cấp thêm lớp tính năng thông minh vượt trội. Hệ thống sẽ không chỉ là nơi lưu trữ thông tin mà còn là một "trợ lý ảo" chủ động phân tích và cung cấp thông tin chi tiết (insight) cho người dùng.

### **2.2. Chức năng sản phẩm**

Các chức năng chính của hệ thống bao gồm:

- **Quản lý Người dùng và Phân quyền:** Đăng ký, đăng nhập, quản lý hồ sơ và vai trò.
- **Quản lý Dự án và Công việc:** Tạo dự án, quản lý các công việc (tasks) theo vòng đời, gán người thực hiện, đặt hạn chót.
- **Tương tác và Cộng tác:** Thảo luận qua bình luận, đính kèm tệp, nhận thông báo thời gian thực.
- **Trợ lý AI Thông minh:** Hỏi-đáp bằng ngôn ngữ tự nhiên, tóm tắt, tạo báo cáo, phân tích và cảnh báo rủi ro.

### **2.3. Đối tượng người dùng**

1. **Quản trị viên (Admin):**
   - Quản lý toàn bộ hệ thống, tài khoản người dùng, và các cài đặt chung.
2. **Quản lý dự án (Project Manager - PM):**
   - Tạo và quản lý dự án, phân công công việc, theo dõi tiến độ tổng thể.
   - Sử dụng các tính năng AI để có cái nhìn tổng quan, phát hiện rủi ro và tạo báo cáo.
3. **Thành viên nhóm (Team Member):**
   - Cập nhật tiến độ công việc được giao, thảo luận với các thành viên khác.
   - Sử dụng AI để hỏi nhanh thông tin về task của mình hoặc các task liên quan.

### **2.4. Môi trường hoạt động**

Hệ thống là một ứng dụng web, có thể truy cập qua các trình duyệt web hiện đại (Google Chrome, Firefox, Safari, Edge) trên máy tính để bàn và các thiết bị di động (thiết kế đáp ứng - responsive).

### **2.5. Ràng buộc thiết kế và triển khai**

- Hệ thống phải được xây dựng trên kiến trúc Microservice.
- Backend services sử dụng **.NET 8 (ASP.NET Core)**, ngoại trừ AI Service sử dụng **Python (FastAPI)**.
- Cơ sở dữ liệu: **PostgreSQL** (với pgvector extension) cho tất cả các microservices để đơn giản hóa vận hành và đảm bảo tính thống nhất.
- Giao tiếp bất đồng bộ giữa các service phải sử dụng **Apache Kafka**.
- Frontend phải được xây dựng bằng **ReactJS hoặc Next.js**.
- Real-time notifications sử dụng **SignalR (WebSocket)**.
- Toàn bộ hệ thống phải được đóng gói bằng **Docker**.
- Quy trình CI/CD phải được tự động hóa bằng **GitHub Actions**.
- Hệ thống AI phải sử dụng **kỹ thuật RAG** với **Google Gemini API** để đảm bảo LLM trả lời dựa trên dữ liệu dự án thực tế.

---

### **3. Yêu cầu chức năng (Functional Requirements)**

### **FR-USER - Module Quản lý Người dùng**

- **FR-USER-01:** Hệ thống cho phép người dùng mới đăng ký tài khoản bằng email và mật khẩu.
- **FR-USER-02:** Hệ thống yêu cầu xác thực email sau khi đăng ký.
- **FR-USER-03:** Người dùng có thể đăng nhập vào hệ thống bằng email và mật khẩu.
- **FR-USER-04:** Người dùng có thể xem và chỉnh sửa thông tin cá nhân (tên, ảnh đại diện).
- **FR-USER-05:** Admin có thể quản lý (thêm/xóa/vô hiệu hóa) tài khoản người dùng.
- **FR-USER-06:** Hệ thống phải có 3 vai trò: Admin, PM, Member với các quyền hạn khác nhau.

### **FR-PROJECT - Module Quản lý Dự án và Công việc**

- **FR-PROJECT-01:** PM có thể tạo một dự án mới với tên, mô tả, và danh sách thành viên.
- **FR-PROJECT-02:** Người dùng có thể xem danh sách các dự án mình là thành viên.
- **FR-PROJECT-03:** Trong một dự án, người dùng có thể tạo một công việc (task) mới với các thông tin: tiêu đề, mô tả, người được giao, ngày hết hạn, độ ưu tiên.
- **FR-PROJECT-04:** Người dùng có thể thay đổi trạng thái của một task (ví dụ: To Do, In Progress, Done).
- **FR-PROJECT-05:** Hệ thống phải cung cấp giao diện xem công việc dưới dạng bảng Kanban và danh sách.
- **FR-PROJECT-06:** Người dùng có thể đính kèm tệp tin vào một task.
- **FR-PROJECT-07:** Người dùng có thể thêm bình luận vào một task để thảo luận.

### **FR-NOTIF - Module Thông báo và Cộng tác**

- **FR-NOTIF-01:** Hệ thống phải gửi thông báo (real-time) cho người dùng khi:
  - Họ được gán vào một task mới.
  - Một task của họ có bình luận mới.
  - Trạng thái của một task họ theo dõi bị thay đổi.
- **FR-NOTIF-02:** Người dùng có thể xem danh sách các thông báo chưa đọc.

### **FR-AI - Module Trợ lý AI Thông minh**

- **FR-AI-01: Hỏi-đáp Ngôn ngữ Tự nhiên:**
  - Hệ thống phải cung cấp một giao diện chat để người dùng có thể đặt câu hỏi.
  - Hệ thống phải sử dụng RAG để truy xuất thông tin liên quan từ cơ sở dữ liệu (tasks, comments,...) và đưa cho LLM.
  - Hệ thống phải trả lời được các câu hỏi như:
    - _"Tóm tắt tiến độ của dự án X?"_
    - _"Những ai đang bị quá tải công việc?"_
    - _"Task #123 đang gặp vấn đề gì?"_
- **FR-AI-02: Tạo Báo cáo Tự động:**
  - Người dùng có thể yêu cầu AI tạo báo cáo bằng câu lệnh tự nhiên.
  - Ví dụ: *"Tạo báo cáo tiến độ tuần này cho team Backend, bao gồm các task đã hoàn thành và các vấn đề còn tồn đọng."*
  - LLM sẽ tổng hợp thông tin từ RAG và sinh ra một văn bản báo cáo có cấu trúc.
- **FR-AI-03: Cảnh báo Thông minh:**
  - Hệ thống phải tự động phân tích dữ liệu để phát hiện các rủi ro tiềm ẩn.
  - Ví dụ: Một task gần đến hạn nhưng chưa có hoạt động, một task có nhiều bình luận mang sắc thái tiêu cực (vấn đề, lỗi,...).
  - Hệ thống sẽ chủ động tạo cảnh báo và gửi đến cho PM.
- **FR-AI-04: Đảm bảo tính chính xác:**
  - Hệ thống phải được thiết kế (thông qua Prompt Engineering) để LLM **chỉ** trả lời dựa trên dữ liệu được RAG cung cấp, và phải trả lời "Tôi không có đủ thông tin" nếu không tìm thấy dữ liệu liên quan.

---

### **4. Yêu cầu phi chức năng (Non-functional Requirements)**

- **NFR-PERF-01 (Hiệu năng):**
  - Thời gian tải các trang thông thường phải dưới 2 giây.
  - Thời gian phản hồi cho các truy vấn AI phải dưới 8 giây.
  - Hệ thống phải hỗ trợ ít nhất 100 người dùng đồng thời.
- **NFR-SECU-01 (Bảo mật):**
  - Mật khẩu người dùng phải được băm (hashed) trước khi lưu vào CSDL.
  - Hệ thống phải áp dụng phân quyền dựa trên vai trò (Role-Based Access Control) để đảm bảo người dùng chỉ truy cập được dữ liệu được phép.
  - Giao tiếp giữa client và server phải được mã hóa bằng HTTPS.
- **NFR-RELI-01 (Độ tin cậy):**
  - Hệ thống phải có thời gian hoạt động (uptime) tối thiểu 99.5%.
  - Dữ liệu người dùng phải được sao lưu định kỳ (hàng ngày).
- **NFR-USAB-01 (Khả năng sử dụng):**
  - Giao diện phải sạch sẽ, trực quan và dễ sử dụng.
  - Hệ thống phải tương thích (responsive) trên các kích thước màn hình phổ biến.

---

### **5. Yêu cầu về Giao diện (Interface Requirements)**

### **5.1. Giao diện Người dùng (User Interfaces)**

- Giao diện sẽ tuân theo một hệ thống thiết kế (design system) nhất quán về màu sắc, font chữ và các thành phần.
- Dashboard chính sẽ hiển thị tổng quan các dự án, các task được gán cho người dùng, và các thông báo mới.
- Giao diện quản lý task sẽ có dạng bảng Kanban kéo-thả và dạng danh sách có thể lọc và sắp xếp.
- Giao diện chat với Trợ lý AI sẽ quen thuộc, tương tự các ứng dụng nhắn tin.

### **5.2. Giao diện Phần mềm (Software Interfaces)**

- Frontend sẽ giao tiếp với Backend thông qua một **API Gateway** duy nhất (YARP hoặc Ocelot) sử dụng REST API.
- Các Microservice nội bộ sẽ giao tiếp với nhau thông qua **REST API** (cho các yêu cầu đồng bộ) và qua **Kafka** (cho các sự kiện bất đồng bộ).
- **AI Service** sẽ giao tiếp với **Google Gemini API** để lấy embeddings và generate responses.
- Hệ thống sẽ sử dụng **PostgreSQL với pgvector extension** để lưu trữ và truy xuất vector embeddings cho RAG.

### **5.3. Giao diện Giao tiếp (Communications Interfaces)**

- Giao thức chính giữa client và server là HTTPS.
- Hệ thống sẽ sử dụng WebSockets để đẩy thông báo thời gian thực từ server xuống client.

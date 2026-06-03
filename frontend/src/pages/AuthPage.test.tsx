import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import AuthPage from "./AuthPage";

const mocks = vi.hoisted(() => ({
  getActiveAccount: vi.fn(),
  getAllAccounts: vi.fn(),
  getGraphAccessToken: vi.fn(),
  isMicrosoftAuthenticated: vi.fn(),
  localLogin: vi.fn(),
  localRegister: vi.fn(),
  loginRedirect: vi.fn(),
  microsoftLogin: vi.fn(),
  microsoftRegister: vi.fn(),
  navigate: vi.fn(),
  setSession: vi.fn(),
  useAuth: vi.fn(),
  useMsal: vi.fn(),
}));

vi.mock("@azure/msal-browser", () => ({
  InteractionStatus: {
    None: "none",
  },
}));

vi.mock("@azure/msal-react", () => ({
  useIsAuthenticated: () => mocks.isMicrosoftAuthenticated(),
  useMsal: () => mocks.useMsal(),
}));

vi.mock("react-router-dom", async (importOriginal) => {
  const actual = await importOriginal<typeof import("react-router-dom")>();

  return {
    ...actual,
    useNavigate: () => mocks.navigate,
  };
});

vi.mock("../api/auth", () => ({
  localLogin: mocks.localLogin,
  localRegister: mocks.localRegister,
  microsoftLogin: mocks.microsoftLogin,
  microsoftRegister: mocks.microsoftRegister,
}));

vi.mock("../auth/msal", () => ({
  getGraphAccessToken: mocks.getGraphAccessToken,
  graphLoginRequest: {
    scopes: ["User.Read"],
  },
}));

vi.mock("../hooks/useAuth", () => ({
  useAuth: () => mocks.useAuth(),
}));

const authResponse = {
  accessToken: "jwt-token",
  email: "employee@example.com",
  fullName: "Employee One",
  role: "Employee",
  userId: "employee-1",
};

const renderAuthPage = () => render(<AuthPage />);

describe("AuthPage", () => {
  beforeEach(() => {
    Object.values(mocks).forEach((mock) => mock.mockReset());
    sessionStorage.clear();

    mocks.getActiveAccount.mockReturnValue({
      homeAccountId: "account-1",
    });
    mocks.getAllAccounts.mockReturnValue([]);
    mocks.getGraphAccessToken.mockResolvedValue("graph-token");
    mocks.isMicrosoftAuthenticated.mockReturnValue(false);
    mocks.localLogin.mockResolvedValue(authResponse);
    mocks.localRegister.mockResolvedValue(authResponse);
    mocks.loginRedirect.mockResolvedValue(undefined);
    mocks.microsoftLogin.mockResolvedValue(authResponse);
    mocks.microsoftRegister.mockResolvedValue(authResponse);
    mocks.useAuth.mockReturnValue({
      isAuthenticated: false,
      setSession: mocks.setSession,
      user: null,
    });
    mocks.useMsal.mockReturnValue({
      accounts: [],
      inProgress: "none",
      instance: {
        getActiveAccount: mocks.getActiveAccount,
        getAllAccounts: mocks.getAllAccounts,
        loginRedirect: mocks.loginRedirect,
      },
    });
  });

  it("starts Microsoft sign-in and stores the login intent", async () => {
    const user = userEvent.setup();

    renderAuthPage();

    await user.click(
      screen.getByRole("button", { name: "Sign in with Microsoft" }),
    );

    expect(sessionStorage.getItem("microsoftAuthIntent")).toBe("login");
    expect(mocks.loginRedirect).toHaveBeenCalledWith({
      prompt: "select_account",
      scopes: ["User.Read"],
    });
  });

  it("finalizes Microsoft login after MSAL authentication", async () => {
    mocks.isMicrosoftAuthenticated.mockReturnValue(true);
    mocks.useMsal.mockReturnValue({
      accounts: [{ homeAccountId: "account-1" }],
      inProgress: "none",
      instance: {
        getActiveAccount: mocks.getActiveAccount,
        getAllAccounts: mocks.getAllAccounts,
        loginRedirect: mocks.loginRedirect,
      },
    });

    renderAuthPage();

    await waitFor(() => {
      expect(mocks.microsoftLogin).toHaveBeenCalledWith("graph-token");
    });
    expect(mocks.setSession).toHaveBeenCalledWith("jwt-token", {
      email: "employee@example.com",
      fullName: "Employee One",
      role: "Employee",
      userId: "employee-1",
    });
    expect(mocks.navigate).toHaveBeenCalledWith("/dashboard", {
      replace: true,
    });
  });

  it("finalizes Microsoft registration when the stored intent is register", async () => {
    sessionStorage.setItem("microsoftAuthIntent", "register");
    mocks.isMicrosoftAuthenticated.mockReturnValue(true);

    renderAuthPage();

    await waitFor(() => {
      expect(mocks.microsoftRegister).toHaveBeenCalledWith("graph-token");
    });
    expect(mocks.microsoftLogin).not.toHaveBeenCalled();
    expect(sessionStorage.getItem("microsoftAuthIntent")).toBeNull();
  });

  it("validates local login before calling the API", async () => {
    const user = userEvent.setup();

    renderAuthPage();

    await user.click(
      screen.getByRole("button", { name: "Use email and password" }),
    );
    await user.click(
      screen.getAllByRole("button", { name: "Sign in" }).at(-1)!,
    );

    expect(
      screen.getByText("Email and password are required."),
    ).toBeInTheDocument();
    expect(mocks.localLogin).not.toHaveBeenCalled();
  });

  it("submits local login credentials and stores the session", async () => {
    const user = userEvent.setup();

    renderAuthPage();

    await user.click(
      screen.getByRole("button", { name: "Use email and password" }),
    );
    await user.type(screen.getByLabelText("Email"), " employee@example.com ");
    await user.type(screen.getByLabelText("Password"), "secret");
    await user.click(
      screen.getAllByRole("button", { name: "Sign in" }).at(-1)!,
    );

    await waitFor(() => {
      expect(mocks.localLogin).toHaveBeenCalledWith(
        "employee@example.com",
        "secret",
      );
    });
    expect(mocks.setSession).toHaveBeenCalledWith("jwt-token", {
      email: "employee@example.com",
      fullName: "Employee One",
      role: "Employee",
      userId: "employee-1",
    });
  });

  it("submits local registration credentials", async () => {
    const user = userEvent.setup();

    renderAuthPage();

    await user.click(
      screen.getByRole("button", { name: "Use email and password" }),
    );
    await user.click(
      screen.getAllByRole("button", { name: "Create account" })[0],
    );
    await user.type(screen.getByLabelText("Full name"), " Employee One ");
    await user.type(screen.getByLabelText("Email"), " employee@example.com ");
    await user.type(screen.getByLabelText("Password"), "secret");
    await user.click(
      screen.getAllByRole("button", { name: "Create account" }).at(-1)!,
    );

    await waitFor(() => {
      expect(mocks.localRegister).toHaveBeenCalledWith(
        "employee@example.com",
        "Employee One",
        "secret",
      );
    });
  });
});
